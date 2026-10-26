using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Contracts;

namespace Sgt.Modules.GamingFloor.Infrastructure.Persistence;

/// <summary>Günlük canlı oyun ve slot özeti (Spec 011 KK3–KK4).</summary>
internal sealed class DailyFloorQuery(FloorDbContext db) : IDailyFloorQuery
{
    public async Task<DailyFloorSummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct)
    {
        var tenant = tenantId.Value;
        var ratings = db.Ratings.AsNoTracking().Where(r => r.TenantId == tenant && r.FinishedAt >= day.Start && r.FinishedAt < day.End);

        var live = await ratings
            .GroupBy(r => r.Currency)
            .Select(g => new LiveCurrencyTotals(g.Key, g.Sum(r => r.TheoreticalWin ?? 0), -g.Sum(r => r.PlayerResult ?? 0)))
            .ToListAsync(ct);

        var slots = await db.MeterReadings.AsNoTracking()
            .Where(r => r.TenantId == tenant && r.ReadAt >= day.Start && r.ReadAt < day.End)
            .Join(db.SlotMachines, r => r.MachineId, m => m.Id, (r, m) => new { m.Currency, r.DeltaCoinIn, r.DeltaCoinOut, r.DeltaJackpot })
            .GroupBy(x => x.Currency)
            .Select(g => new SlotCurrencyTotals(g.Key, g.Sum(x => x.DeltaCoinIn), g.Sum(x => x.DeltaCoinIn - x.DeltaCoinOut - x.DeltaJackpot)))
            .ToListAsync(ct);

        return new DailyFloorSummary(
            await ratings.CountAsync(ct),
            live.OrderBy(l => l.Currency, StringComparer.Ordinal).ToList(),
            slots.OrderBy(s => s.Currency, StringComparer.Ordinal).ToList());
    }
}
