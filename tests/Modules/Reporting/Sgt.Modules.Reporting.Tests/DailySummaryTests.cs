using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Contracts;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.Loyalty.Contracts;
using Sgt.Modules.Reporting.Application.Daily;

namespace Sgt.Modules.Reporting.Tests;

public sealed class DailySummaryTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());

    [Fact]
    public async Task KK1_KK7_default_day_is_today_in_kktc_and_sections_come_from_modules()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 10, 26, 22, 30, 0, TimeSpan.Zero)); // KKTC: 27 Ekim 01:30
        var cage = new RecordingCage();

        var summary = await new GetDailySummaryHandler(cage, new EmptyFloor(), new EmptyLoyalty(), clock)
            .Handle(new GetDailySummaryQuery(Tenant, null), default);

        Assert.Equal(new DateOnly(2026, 10, 27), summary.Date);
        Assert.Equal(new DateTimeOffset(2026, 10, 26, 22, 0, 0, TimeSpan.Zero), summary.From.ToUniversalTime()); // Ekim sonu: EET (UTC+2)
        Assert.Equal(summary.From.AddDays(1), summary.To);
        Assert.Equal(summary.Date, cage.AskedFor!.Date);
        Assert.Equal(3, summary.Cage.ShiftsOpen);
    }

    [Fact]
    public async Task KK7_past_day_can_be_requested()
    {
        var summary = await new GetDailySummaryHandler(new RecordingCage(), new EmptyFloor(), new EmptyLoyalty(), new FixedClock(DateTimeOffset.UtcNow))
            .Handle(new GetDailySummaryQuery(Tenant, new DateOnly(2026, 1, 15)), default);

        Assert.Equal(new DateOnly(2026, 1, 15), summary.Date);
    }

    private sealed class RecordingCage : IDailyCageQuery
    {
        public BusinessDay? AskedFor { get; private set; }

        public Task<DailyCageSummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct)
        {
            AskedFor = day;
            return Task.FromResult(new DailyCageSummary(3, 0, 0, []));
        }
    }

    private sealed class EmptyFloor : IDailyFloorQuery
    {
        public Task<DailyFloorSummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct) => Task.FromResult(new DailyFloorSummary(0, [], []));
    }

    private sealed class EmptyLoyalty : IDailyLoyaltyQuery
    {
        public Task<DailyLoyaltySummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct) => Task.FromResult(new DailyLoyaltySummary(0, 0));
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
