using System.Globalization;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.Configuration.Application.ExchangeRates;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.GamingFloor.Application.Slots;
using Sgt.Modules.GamingFloor.Application.Tables;
using Sgt.DemoData;
using Sgt.Modules.Reporting.Application.Daily;

namespace Sgt.Api.Tests.DemoData;

/// <summary>Spec 012 US1: Small ölçekli demo verisi (KK2–KK5, KK7).</summary>
[Collection(DemoSeedGroup.Name)]
public sealed class DemoSeedTests(DemoSeedFixture fx)
{
    private static readonly Sgt.BuildingBlocks.TenantId Tenant = ApiFactory.Tenant;

    [Fact]
    public void KK2_first_run_seeds_data()
    {
        Assert.True(fx.FirstRun.Seeded);
    }

    [Fact]
    public async Task KK3_players_are_registered_with_cards_and_some_are_restricted()
    {
        var players = new List<PlayerSummary>();
        for (var i = 1; i <= DemoScale.Small.Players; i++)
        {
            players.Add(Assert.Single(await SearchPlayer($"P{i:D6}")));
        }

        Assert.Empty(await SearchPlayer($"P{DemoScale.Small.Players + 1:D6}"));
        Assert.All(players, p => Assert.NotNull(p.ActiveCardNumber));
        Assert.Contains(players, p => p.IsRestricted);
        Assert.Contains(players, p => !p.IsRestricted);
    }

    [Fact]
    public async Task KK3_floor_and_rates_are_defined()
    {
        var tables = await fx.Query<ListTablesQuery, IReadOnlyList<TableView>>(new ListTablesQuery(Tenant));
        var slots = await fx.Query<GetSlotFloorQuery, IReadOnlyList<SlotFloorRow>>(new GetSlotFloorQuery(Tenant));
        var rates = await fx.Query<GetCurrentRatesQuery, IReadOnlyList<ExchangeRateDto>>(new GetCurrentRatesQuery(Tenant));

        Assert.Equal(DemoScale.Small.Tables, tables.Count);
        Assert.Equal(DemoScale.Small.SlotMachines, slots.Count);
        Assert.Equal(["EUR", "GBP", "USD"], rates.Select(r => r.Currency).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task KK4_daily_summary_is_filled_for_every_simulated_day()
    {
        for (var day = fx.Api.Timeline.FirstDay; day < fx.Api.Timeline.Today; day = day.AddDays(1))
        {
            var summary = await fx.Query<GetDailySummaryQuery, DailySummary>(new GetDailySummaryQuery(Tenant, day));
            var label = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            Assert.True(summary.Cage.ShiftsClosed >= 1, $"{label}: kapanan vardiya yok");
            Assert.Contains(summary.Cage.Currencies, c => c.Currency == "TRY" && c.ChipSales > 0);
            Assert.True(summary.Floor.RatingsFinished > 0, $"{label}: rating yok");
            Assert.NotEmpty(summary.Floor.Slots);
            Assert.True(summary.Loyalty.PointsEarned > 0, $"{label}: puan kazanımı outbox ile Loyalty'ye geçmedi");
        }
    }

    [Fact]
    public async Task KK5_data_is_deterministic_from_fixed_seed()
    {
        var expected = new SyntheticNames(DemoRun.Seed).NextPerson(fx.Api.Timeline.Today);

        var first = Assert.Single(await SearchPlayer("P000001"));

        Assert.Equal(expected.FirstName, first.FirstName);
        Assert.Equal(expected.LastName, first.LastName);
        Assert.Equal(SyntheticNames.CardNumber(1), first.ActiveCardNumber);
    }

    [Fact]
    public async Task KK5_second_run_writes_nothing()
    {
        var lastDay = fx.Api.Timeline.Today.AddDays(-1);
        var before = await fx.Query<GetDailySummaryQuery, DailySummary>(new GetDailySummaryQuery(Tenant, lastDay));

        var second = await fx.Generate();

        var after = await fx.Query<GetDailySummaryQuery, DailySummary>(new GetDailySummaryQuery(Tenant, lastDay));
        Assert.False(second.Seeded);
        Assert.Empty(await SearchPlayer($"P{DemoScale.Small.Players + 1:D6}"));
        Assert.Equal(before.Floor.RatingsFinished, after.Floor.RatingsFinished);
        Assert.Equal(before.Loyalty, after.Loyalty);
    }

    private Task<IReadOnlyList<PlayerSummary>> SearchPlayer(string number) =>
        fx.Query<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>(new SearchPlayersQuery(Tenant, number));
}
