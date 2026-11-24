using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Domain;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Infrastructure.Persistence;

internal static class SlotMapping
{
    public static SlotMachine ToDomain(SlotMachineRow r) => SlotMachine.Restore(
        r.Id, new TenantId(r.TenantId), r.AssetNumber, r.Area, r.Manufacturer, r.Model, r.Denomination, r.Currency, r.LastReadAt,
        r.LastReadAt is null ? null : new MeterSnapshot(r.LastCoinIn ?? 0, r.LastCoinOut ?? 0, r.LastJackpot ?? 0, r.LastGamesPlayed ?? 0));

    public static void CopyTo(SlotMachine m, SlotMachineRow r)
    {
        r.Id = m.Id;
        r.TenantId = m.TenantId.Value;
        r.AssetNumber = m.AssetNumber;
        r.Area = m.Area;
        r.Manufacturer = m.Manufacturer;
        r.Model = m.Model;
        r.Denomination = m.Denomination;
        r.Currency = m.Currency;
        r.LastReadAt = m.LastReadAt;
        r.LastCoinIn = m.LastMeters?.CoinIn;
        r.LastCoinOut = m.LastMeters?.CoinOut;
        r.LastJackpot = m.LastMeters?.Jackpot;
        r.LastGamesPlayed = m.LastMeters?.GamesPlayed;
    }

    public static MeterReadingRow ToRow(TenantId tenantId, RecordedReading x) => new()
    {
        Id = x.Reading.Id, TenantId = tenantId.Value, MachineId = x.MachineId, ReadAt = x.Reading.ReadAt,
        CoinIn = x.Reading.Meters.CoinIn, CoinOut = x.Reading.Meters.CoinOut, Jackpot = x.Reading.Meters.Jackpot, GamesPlayed = x.Reading.Meters.GamesPlayed,
        DeltaCoinIn = x.Delta.CoinIn, DeltaCoinOut = x.Delta.CoinOut, DeltaJackpot = x.Delta.Jackpot, DeltaGamesPlayed = x.Delta.GamesPlayed,
        Source = x.Reading.Source.ToString(), SourceReference = x.Reading.SourceReference, IsReset = x.Reading.IsReset, Note = x.Reading.Note,
        ActorId = x.Reading.Audit.Actor.Value, Terminal = x.Reading.Audit.Terminal, ReasonCode = x.Reading.Audit.Reason.Value,
        CorrelationId = x.Reading.Audit.CorrelationId,
    };

    public static RecordedReading ToDomain(MeterReadingRow r) => new(
        r.MachineId,
        new MeterReading(
            r.Id, r.ReadAt, new MeterSnapshot(r.CoinIn, r.CoinOut, r.Jackpot, r.GamesPlayed), Enum.Parse<ReadingSource>(r.Source), r.SourceReference,
            r.IsReset, r.Note, new AuditContext(new UserId(r.ActorId), r.Terminal, ReasonCode.Of(r.ReasonCode), r.CorrelationId)),
        new MeterSnapshot(r.DeltaCoinIn, r.DeltaCoinOut, r.DeltaJackpot, r.DeltaGamesPlayed));
}

internal sealed class SlotMachineRepository(FloorDbContext db, TrackedFloor tracked) : ISlotMachineRepository
{
    public Task<SlotMachine?> Get(TenantId tenantId, Guid machineId, CancellationToken ct) =>
        Load(m => m.TenantId == tenantId.Value && m.Id == machineId, ct);

    public Task<SlotMachine?> GetByAsset(TenantId tenantId, string assetNumber, CancellationToken ct)
    {
        var asset = assetNumber.Trim().ToUpperInvariant();
        return Load(m => m.TenantId == tenantId.Value && m.AssetNumber == asset, ct);
    }

    public Task<bool> AssetExists(TenantId tenantId, string assetNumber, CancellationToken ct) =>
        db.SlotMachines.AnyAsync(m => m.TenantId == tenantId.Value && m.AssetNumber == assetNumber, ct);

    public Task<bool> ReadingExists(TenantId tenantId, ReadingSource source, string reference, CancellationToken ct)
    {
        var sourceName = source.ToString();
        return db.MeterReadings.AnyAsync(r => r.TenantId == tenantId.Value && r.Source == sourceName && r.SourceReference == reference, ct);
    }

    public async Task<IReadOnlyList<SlotMachine>> List(TenantId tenantId, CancellationToken ct) =>
        (await db.SlotMachines.AsNoTracking().Where(m => m.TenantId == tenantId.Value).ToListAsync(ct)).Select(SlotMapping.ToDomain).ToList();

    public async Task<IReadOnlyList<RecordedReading>> ReadingsBetween(TenantId tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
        (await db.MeterReadings.AsNoTracking().Where(r => r.TenantId == tenantId.Value && r.ReadAt >= from && r.ReadAt < to).ToListAsync(ct))
        .Select(SlotMapping.ToDomain).ToList();

    public async Task<IReadOnlyList<RecordedReading>> LatestReadings(TenantId tenantId, Guid machineId, int take, CancellationToken ct) =>
        (await db.MeterReadings.AsNoTracking().Where(r => r.TenantId == tenantId.Value && r.MachineId == machineId)
            .OrderByDescending(r => r.ReadAt).Take(take).ToListAsync(ct))
        .Select(SlotMapping.ToDomain).ToList();

    public void Add(SlotMachine machine) => tracked.Machines[machine] = null;

    public void AddReading(RecordedReading reading) => tracked.Readings.Add(reading);

    private async Task<SlotMachine?> Load(System.Linq.Expressions.Expression<Func<SlotMachineRow, bool>> filter, CancellationToken ct)
    {
        var matches = filter.Compile();
        var known = tracked.Machines.FirstOrDefault(p => p.Value is not null && matches(p.Value)).Key;
        if (known is not null)
        {
            return known;
        }

        var row = await db.SlotMachines.SingleOrDefaultAsync(filter, ct);
        if (row is null)
        {
            return null;
        }

        var machine = SlotMapping.ToDomain(row);
        tracked.Machines[machine] = row;
        return machine;
    }
}

internal sealed class FloorEvents(TrackedFloor tracked) : IFloorEvents
{
    public void Publish(TenantId tenantId, FloorChange change) => tracked.ExtraChanges.Add((tenantId, change));
}

internal sealed class FloorInbox(FloorDbContext db, IClock clock) : EfInbox<FloorDbContext>(db, clock), IFloorInbox;
