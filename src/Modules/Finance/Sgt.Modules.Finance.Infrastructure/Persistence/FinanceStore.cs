using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Contracts;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Infrastructure.Persistence;

internal sealed class TrackedShifts
{
    public Dictionary<CashierShift, ShiftRow?> Items { get; } = [];

    public Dictionary<CageAdjustmentRequest, AdjustmentRequestRow?> Adjustments { get; } = [];
}

internal static class FinanceMapping
{
    public static CashierShift ToDomain(ShiftRow row) =>
        CashierShift.Restore(
            row.Id, new TenantId(row.TenantId), new UserId(row.CashierId), row.Terminal, row.OpenedAt, row.ClosedAt, row.ClosingNote,
            row.Opening.Select(o => new DrawerAmount(Enum.Parse<Instrument>(o.Instrument), o.Currency, o.Amount)),
            row.Transactions.OrderBy(t => t.OccurredAt).Select(ToDomain),
            row.ClosingCounts.Select(c => new ClosingCount(Enum.Parse<Instrument>(c.Instrument), c.Currency, c.Expected, c.Counted)));

    public static CageTransaction ToDomain(CageTransactionRow t) =>
        CageTransaction.Restore(
            t.Id, Enum.Parse<CageTransactionType>(t.Type), t.PlayerId, Money.Of(t.Amount, t.Currency),
            t.Entries.Select(e => new LedgerEntry(Enum.Parse<LedgerAccount>(e.Account), Enum.Parse<Instrument>(e.Instrument), e.Currency, e.Amount)).ToList(),
            t.Rate,
            new AuditContext(new UserId(t.ActorId), t.Terminal, ReasonCode.Of(t.ReasonCode), t.CorrelationId),
            t.OccurredAt,
            t.ReversesId);

    /// <summary>Domain'deki yeni satırları ekler; mevcut değer satırlarına dokunmaz (append-only).</summary>
    public static void CopyTo(CashierShift shift, ShiftRow row)
    {
        row.Id = shift.Id;
        row.TenantId = shift.TenantId.Value;
        row.CashierId = shift.CashierId.Value;
        row.Terminal = shift.Terminal;
        row.OpenedAt = shift.OpenedAt;
        row.ClosedAt = shift.ClosedAt;
        row.ClosingNote = shift.ClosingNote;
        row.TransactionCount = shift.Transactions.Count;

        if (row.Opening.Count == 0)
        {
            row.Opening.AddRange(shift.Opening.Select(o => new OpeningAmountRow
            {
                ShiftId = shift.Id, Instrument = o.Instrument.ToString(), Currency = o.Currency, Amount = o.Amount,
            }));
        }

        var persisted = row.Transactions.Select(t => t.Id).ToHashSet();
        foreach (var t in shift.Transactions.Where(t => !persisted.Contains(t.Id)))
        {
            row.Transactions.Add(new CageTransactionRow
            {
                Id = t.Id, TenantId = shift.TenantId.Value, ShiftId = shift.Id, Type = t.Type.ToString(), PlayerId = t.PlayerId,
                Amount = t.Amount.Amount, Currency = t.Amount.Currency, Rate = t.Rate,
                ActorId = t.Audit.Actor.Value, Terminal = t.Audit.Terminal, ReasonCode = t.Audit.Reason.Value, CorrelationId = t.Audit.CorrelationId,
                OccurredAt = t.OccurredAt,
                ReversesId = t.ReversesId,
                Entries = t.Entries.Select(e => new LedgerEntryRow
                {
                    Id = Guid.NewGuid(), TransactionId = t.Id, Account = e.Account.ToString(), Instrument = e.Instrument.ToString(),
                    Currency = e.Currency, Amount = e.Amount,
                }).ToList(),
            });
        }

        if (row.ClosingCounts.Count == 0)
        {
            row.ClosingCounts.AddRange(shift.ClosingCounts.Select(c => new ClosingCountRow
            {
                ShiftId = shift.Id, Instrument = c.Instrument.ToString(), Currency = c.Currency, Expected = c.Expected, Counted = c.Counted,
            }));
        }
    }

    public static CageShiftChangedV1 ToEvent(CashierShift shift, ShiftChange change) => new(
        change.Id, change.Audit.CorrelationId, change.OccurredAt, shift.TenantId, change.Audit, shift.Id, change.TransactionId,
        change.Kind.ToString(), change.Detail);
}

internal sealed class ShiftRepository(FinanceDbContext db, TrackedShifts tracked) : IShiftRepository
{
    public async Task<CashierShift?> Get(TenantId tenantId, Guid shiftId, CancellationToken ct)
    {
        var row = await db.Shifts
            .Include(s => s.Opening)
            .Include(s => s.Transactions).ThenInclude(t => t.Entries)
            .Include(s => s.ClosingCounts)
            .AsSplitQuery()
            .SingleOrDefaultAsync(s => s.TenantId == tenantId.Value && s.Id == shiftId, ct);
        if (row is null)
        {
            return null;
        }

        var shift = FinanceMapping.ToDomain(row);
        tracked.Items[shift] = row;
        return shift;
    }

    public Task<bool> HasOpenShift(TenantId tenantId, UserId cashier, string terminal, CancellationToken ct) =>
        db.Shifts.AnyAsync(s => s.TenantId == tenantId.Value && s.ClosedAt == null && (s.CashierId == cashier.Value || s.Terminal == terminal), ct);

    public async Task<TransactionLookup?> FindTransaction(TenantId tenantId, Guid transactionId, CancellationToken ct)
    {
        var row = await db.Transactions.AsNoTracking().Include(t => t.Entries)
            .SingleOrDefaultAsync(t => t.TenantId == tenantId.Value && t.Id == transactionId, ct);
        return row is null ? null : new TransactionLookup(row.ShiftId, FinanceMapping.ToDomain(row));
    }

    public async Task<Guid?> FindOpenShiftId(TenantId tenantId, UserId cashier, CancellationToken ct) =>
        await db.Shifts.Where(s => s.TenantId == tenantId.Value && s.CashierId == cashier.Value && s.ClosedAt == null)
            .Select(s => (Guid?)s.Id).SingleOrDefaultAsync(ct);

    public void Add(CashierShift shift) => tracked.Items[shift] = null;
}

internal sealed class CageAdjustmentRepository(FinanceDbContext db, TrackedShifts tracked) : ICageAdjustmentRepository
{
    public async Task<CageAdjustmentRequest?> Get(TenantId tenantId, Guid requestId, CancellationToken ct)
    {
        var row = await db.AdjustmentRequests.SingleOrDefaultAsync(r => r.TenantId == tenantId.Value && r.Id == requestId, ct);
        if (row is null)
        {
            return null;
        }

        var request = CageAdjustmentRequest.Restore(
            row.Id, tenantId, row.ShiftId, row.TransactionId, row.Note, row.RequestedAt,
            FourEyesDecision.Restore(new UserId(row.RequestedBy), Enum.Parse<DecisionStatus>(row.Status), row.DecidedBy is { } by ? new UserId(by) : null, row.DecidedAt),
            row.ReversalId);
        tracked.Adjustments[request] = row;
        return request;
    }

    public void Add(CageAdjustmentRequest request) => tracked.Adjustments[request] = null;
}

internal sealed class FinanceUnitOfWork(FinanceDbContext db, TrackedShifts tracked, IntegrationEventSerializer serializer) : IFinanceUnitOfWork
{
    public async Task Commit(CancellationToken ct)
    {
        foreach (var (shift, existing) in tracked.Items.ToList())
        {
            var row = existing ?? new ShiftRow();
            if (existing is null)
            {
                db.Shifts.Add(row);
                tracked.Items[shift] = row;
            }

            FinanceMapping.CopyTo(shift, row);
            foreach (var change in shift.PendingChanges)
            {
                db.Set<OutboxMessage>().Add(serializer.ToOutbox(FinanceMapping.ToEvent(shift, change)));
            }
        }

        foreach (var (request, existing) in tracked.Adjustments.ToList())
        {
            var row = existing ?? db.AdjustmentRequests.Add(new AdjustmentRequestRow()).Entity;
            tracked.Adjustments[request] = row;
            row.Id = request.Id;
            row.TenantId = request.TenantId.Value;
            row.ShiftId = request.ShiftId;
            row.TransactionId = request.TransactionId;
            row.Note = request.Note;
            row.RequestedAt = request.RequestedAt;
            row.RequestedBy = request.Decision.RequestedBy.Value;
            row.Status = request.Decision.Status.ToString();
            row.DecidedBy = request.Decision.DecidedBy?.Value;
            row.DecidedAt = request.Decision.DecidedAt;
            row.ReversalId = request.ReversalId;
            foreach (var change in request.PendingChanges)
            {
                db.Set<OutboxMessage>().Add(serializer.ToOutbox(new CageShiftChangedV1(
                    change.Id, change.Audit.CorrelationId, change.OccurredAt, request.TenantId, change.Audit, request.ShiftId, change.TransactionId,
                    change.Kind.ToString(), change.Detail)));
            }

            request.ClearPendingChanges();
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyConflictException("Vardiya aynı anda başka bir işlemle değişti; tekrar deneyin.", e);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConcurrencyConflictException("Aynı işlem anahtarı veya açık vardiya aynı anda kaydedildi.", e);
        }

        foreach (var shift in tracked.Items.Keys)
        {
            shift.ClearPendingChanges();
        }
    }
}

internal sealed class DesignTimeFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<FinanceDbContext>
{
    public FinanceDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<FinanceDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", FinanceDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}
