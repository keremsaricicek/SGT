using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Contracts;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Infrastructure.Persistence;

/// <summary>Günlük puan toplamı (Spec 011 KK5).</summary>
internal sealed class DailyLoyaltyQuery(LoyaltyDbContext db) : IDailyLoyaltyQuery
{
    public async Task<DailyLoyaltySummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct)
    {
        var earned = nameof(PointsEntryKind.Earned);
        var redeemed = nameof(PointsEntryKind.Redeemed);
        var entries = db.Set<PointsEntryRow>().AsNoTracking()
            .Where(e => e.TenantId == tenantId.Value && e.At >= day.Start && e.At < day.End);

        return new DailyLoyaltySummary(
            await entries.Where(e => e.Kind == earned).SumAsync(e => e.Points, ct),
            -await entries.Where(e => e.Kind == redeemed).SumAsync(e => e.Points, ct));
    }
}
