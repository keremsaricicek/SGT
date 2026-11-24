using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Contracts;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.Loyalty.Contracts;

namespace Sgt.Modules.Reporting.Application.Daily;

/// <summary>Tarih verilmezse bugün (KKTC) — Spec 011 KK7.</summary>
public sealed record GetDailySummaryQuery(TenantId TenantId, DateOnly? Date);

public sealed record DailySummary(
    DateOnly Date, DateTimeOffset From, DateTimeOffset To, DailyCageSummary Cage, DailyFloorSummary Floor, DailyLoyaltySummary Loyalty);

/// <summary>Günlük casino özeti (Spec 011). Her bölüm ilgili modülün sözleşmesinden gelir; tablolara erişilmez.</summary>
public sealed class GetDailySummaryHandler(IDailyCageQuery cage, IDailyFloorQuery floor, IDailyLoyaltyQuery loyalty, IClock clock)
    : IQueryHandler<GetDailySummaryQuery, DailySummary>
{
    public async Task<DailySummary> Handle(GetDailySummaryQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var day = BusinessDay.For(query.Date ?? clock.LocalToday());
        return new DailySummary(
            day.Date,
            day.Start,
            day.End,
            await cage.For(query.TenantId, day, ct),
            await floor.For(query.TenantId, day, ct),
            await loyalty.For(query.TenantId, day, ct));
    }
}
