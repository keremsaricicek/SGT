using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.GamingFloor.Domain;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Infrastructure.Persistence;

internal sealed class TrackedFloor
{
    public Dictionary<GamingTable, TableRow?> Tables { get; } = [];

    public Dictionary<Rating, RatingRow?> Ratings { get; } = [];

    public Dictionary<SlotMachine, SlotMachineRow?> Machines { get; } = [];

    public List<RecordedReading> Readings { get; } = [];

    public List<(TenantId TenantId, FloorChange Change)> ExtraChanges { get; } = [];
}

internal static class FloorMapping
{
    public static GamingTable ToDomain(TableRow r) => GamingTable.Restore(
        r.Id, new TenantId(r.TenantId), r.Code, Enum.Parse<GameType>(r.GameType), r.Currency, r.MinBet, r.MaxBet, r.HouseEdgePercent, r.HandsPerHour,
        r.Sessions.OrderBy(s => s.OpenedAt).Select(s => new TableSession(s.Id, s.OpenedAt, new UserId(s.OpenedBy), s.ClosedAt, s.ClosedBy is { } c ? new UserId(c) : null)));

    public static Rating ToDomain(RatingRow r) => Rating.Restore(
        r.Id, new TenantId(r.TenantId), r.PlayerId, r.TableId, r.TableCode, r.Currency, r.HouseEdgePercent, r.HandsPerHour, r.StartedAt,
        new UserId(r.StartedBy), r.FinishedAt, r.AverageBet, r.PlayerResult, r.TheoreticalWin,
        r.Corrections.OrderBy(c => c.At).Select(c => new RatingCorrection(
            c.Id, c.PreviousAverageBet, c.PreviousPlayerResult, c.AverageBet, c.PlayerResult,
            new AuditContext(new UserId(c.ActorId), c.Terminal, ReasonCode.Of(c.ReasonCode), c.CorrelationId), c.At)));

    public static void CopyTo(GamingTable t, TableRow r)
    {
        r.Id = t.Id;
        r.TenantId = t.TenantId.Value;
        r.Code = t.Code;
        r.GameType = t.GameType.ToString();
        r.Currency = t.Currency;
        r.MinBet = t.MinBet;
        r.MaxBet = t.MaxBet;
        r.HouseEdgePercent = t.HouseEdgePercent;
        r.HandsPerHour = t.HandsPerHour;
        foreach (var s in t.Sessions)
        {
            var row = r.Sessions.SingleOrDefault(x => x.Id == s.Id);
            if (row is null)
            {
                r.Sessions.Add(new TableSessionRow { Id = s.Id, TableId = t.Id, OpenedAt = s.OpenedAt, OpenedBy = s.OpenedBy.Value, ClosedAt = s.ClosedAt, ClosedBy = s.ClosedBy?.Value });
            }
            else
            {
                row.ClosedAt = s.ClosedAt;
                row.ClosedBy = s.ClosedBy?.Value;
            }
        }
    }

    public static void CopyTo(Rating x, RatingRow r)
    {
        r.Id = x.Id;
        r.TenantId = x.TenantId.Value;
        r.PlayerId = x.PlayerId;
        r.TableId = x.TableId;
        r.TableCode = x.TableCode;
        r.Currency = x.Currency;
        r.HouseEdgePercent = x.HouseEdgePercent;
        r.HandsPerHour = x.HandsPerHour;
        r.StartedAt = x.StartedAt;
        r.StartedBy = x.StartedBy.Value;
        r.FinishedAt = x.FinishedAt;
        r.AverageBet = x.AverageBet;
        r.PlayerResult = x.PlayerResult;
        r.TheoreticalWin = x.TheoreticalWin;
        var persisted = r.Corrections.Select(c => c.Id).ToHashSet();
        r.Corrections.AddRange(x.Corrections.Where(c => !persisted.Contains(c.Id)).Select(c => new RatingCorrectionRow
        {
            Id = c.Id, RatingId = x.Id, PreviousAverageBet = c.PreviousAverageBet, PreviousPlayerResult = c.PreviousPlayerResult,
            AverageBet = c.AverageBet, PlayerResult = c.PlayerResult, ActorId = c.Audit.Actor.Value, Terminal = c.Audit.Terminal,
            ReasonCode = c.Audit.Reason.Value, CorrelationId = c.Audit.CorrelationId, At = c.At,
        }));
    }

    /// <summary>Rating bitişi/düzeltmesi PlayerRatedV1 (loyalty), diğerleri FloorChangedV1 olarak yayınlanır.</summary>
    public static IIntegrationEvent ToEvent(TenantId tenantId, FloorChange change, Rating? rating) =>
        rating is not null && change.Kind is FloorChangeKind.RatingFinished or FloorChangeKind.RatingCorrected
            ? new PlayerRatedV1(
                change.Id, change.Audit.CorrelationId, change.OccurredAt, tenantId, change.Audit, rating.Id, rating.PlayerId, rating.TableCode,
                rating.Currency, rating.AverageBet!.Value, rating.PlayerResult, rating.TheoreticalWin!.Value, rating.StartedAt, rating.FinishedAt!.Value,
                change.Kind == FloorChangeKind.RatingCorrected)
            : new FloorChangedV1(change.Id, change.Audit.CorrelationId, change.OccurredAt, tenantId, change.Audit, change.Kind.ToString(), change.Detail);
}

internal sealed class TableRepository(FloorDbContext db, TrackedFloor tracked) : ITableRepository
{
    public async Task<GamingTable?> Get(TenantId tenantId, Guid tableId, CancellationToken ct)
    {
        var row = await db.Tables.Include(t => t.Sessions).SingleOrDefaultAsync(t => t.TenantId == tenantId.Value && t.Id == tableId, ct);
        if (row is null)
        {
            return null;
        }

        var table = FloorMapping.ToDomain(row);
        tracked.Tables[table] = row;
        return table;
    }

    public Task<bool> CodeExists(TenantId tenantId, string code, CancellationToken ct) =>
        db.Tables.AnyAsync(t => t.TenantId == tenantId.Value && t.Code == code, ct);

    public async Task<IReadOnlyList<GamingTable>> List(TenantId tenantId, CancellationToken ct) =>
        (await db.Tables.AsNoTracking().Include(t => t.Sessions.Where(s => s.ClosedAt == null)).Where(t => t.TenantId == tenantId.Value).ToListAsync(ct))
        .Select(FloorMapping.ToDomain).ToList();

    public void Add(GamingTable table) => tracked.Tables[table] = null;
}

internal sealed class RatingRepository(FloorDbContext db, TrackedFloor tracked) : IRatingRepository
{
    public Task<Rating?> Get(TenantId tenantId, Guid ratingId, CancellationToken ct) =>
        Load(r => r.TenantId == tenantId.Value && r.Id == ratingId, ct);

    public Task<Rating?> RunningForPlayer(TenantId tenantId, Guid playerId, CancellationToken ct) =>
        Load(r => r.TenantId == tenantId.Value && r.PlayerId == playerId && r.FinishedAt == null, ct);

    public Task<int> RunningAtTable(TenantId tenantId, Guid tableId, CancellationToken ct) =>
        db.Ratings.CountAsync(r => r.TenantId == tenantId.Value && r.TableId == tableId && r.FinishedAt == null, ct);

    public async Task<IReadOnlyList<Rating>> Running(TenantId tenantId, Guid? tableId, CancellationToken ct) =>
        (await db.Ratings.AsNoTracking()
            .Where(r => r.TenantId == tenantId.Value && r.FinishedAt == null && (tableId == null || r.TableId == tableId))
            .OrderBy(r => r.StartedAt).ToListAsync(ct))
        .Select(FloorMapping.ToDomain).ToList();

    public void Add(Rating rating) => tracked.Ratings[rating] = null;

    private async Task<Rating?> Load(System.Linq.Expressions.Expression<Func<RatingRow, bool>> filter, CancellationToken ct)
    {
        var row = await db.Ratings.Include(r => r.Corrections).SingleOrDefaultAsync(filter, ct);
        if (row is null)
        {
            return null;
        }

        var rating = FloorMapping.ToDomain(row);
        tracked.Ratings[rating] = row;
        return rating;
    }
}

internal sealed class FloorUnitOfWork(FloorDbContext db, TrackedFloor tracked, IntegrationEventSerializer serializer) : IFloorUnitOfWork
{
    public async Task Commit(CancellationToken ct)
    {
        foreach (var (table, existing) in tracked.Tables.ToList())
        {
            var row = existing ?? db.Tables.Add(new TableRow()).Entity;
            tracked.Tables[table] = row;
            FloorMapping.CopyTo(table, row);
            Publish(table.TenantId, table.PendingChanges, null);
        }

        foreach (var (rating, existing) in tracked.Ratings.ToList())
        {
            var row = existing ?? db.Ratings.Add(new RatingRow()).Entity;
            tracked.Ratings[rating] = row;
            FloorMapping.CopyTo(rating, row);
            Publish(rating.TenantId, rating.PendingChanges, rating);
        }

        foreach (var (machine, existing) in tracked.Machines.ToList())
        {
            var row = existing ?? db.SlotMachines.Add(new SlotMachineRow()).Entity;
            tracked.Machines[machine] = row;
            SlotMapping.CopyTo(machine, row);
            Publish(machine.TenantId, machine.PendingChanges, null);
            machine.ClearPendingChanges();
        }

        foreach (var reading in tracked.Readings)
        {
            var tenant = tracked.Machines.Keys.Single(m => m.Id == reading.MachineId).TenantId;
            db.MeterReadings.Add(SlotMapping.ToRow(tenant, reading));
        }

        tracked.Readings.Clear();
        foreach (var (tenantId, change) in tracked.ExtraChanges)
        {
            Publish(tenantId, [change], null);
        }

        tracked.ExtraChanges.Clear();

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyConflictException("Masa, rating veya makine aynı anda başka bir işlemle değiştirildi.", e);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConcurrencyConflictException("Aynı masa kodu, açık masa, süren rating veya okuma aynı anda kaydedildi.", e);
        }

        foreach (var table in tracked.Tables.Keys)
        {
            table.ClearPendingChanges();
        }

        foreach (var rating in tracked.Ratings.Keys)
        {
            rating.ClearPendingChanges();
        }
    }

    private void Publish(TenantId tenantId, IEnumerable<FloorChange> changes, Rating? rating)
    {
        foreach (var change in changes)
        {
            db.Set<OutboxMessage>().Add(serializer.ToOutbox(FloorMapping.ToEvent(tenantId, change, rating)));
        }
    }
}

internal sealed class DesignTimeFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<FloorDbContext>
{
    public FloorDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<FloorDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", FloorDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}
