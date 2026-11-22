using System.Globalization;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.Configuration.Application.ExchangeRates;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.GamingFloor.Application.Slots;
using Sgt.Modules.GamingFloor.Application.Tables;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.DemoData;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.CasinoPlayer.Domain;
using Sgt.Modules.Configuration.Application.Markers;
using Sgt.Modules.GamingFloor.Application.Ratings;
using Sgt.Modules.Identity.Application.Panel;
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

    [Fact]
    public async Task KK5_completion_marker_is_written()
    {
        Assert.True(await fx.Query<IsTenantMarkerSetQuery, bool>(new IsTenantMarkerSetQuery(Tenant, DemoDataGenerator.CompletionMarker)));
    }

    [Fact]
    public async Task R3_every_module_outbox_is_empty_after_seed()
    {
        foreach (var drainer in fx.Api.Services.GetRequiredService<OutboxDrainers>())
        {
            var backlog = await drainer.Backlog(Tenant, default);
            Assert.True(backlog.Pending == 0, $"{drainer.Module}: {backlog.Pending} işlenmemiş olay");
            Assert.True(backlog.DeadLettered == 0, $"{drainer.Module}: {backlog.DeadLettered} dead-letter");
        }
    }

    [Fact]
    public async Task KK4_today_looks_alive_with_open_tables_running_ratings_and_an_open_shift()
    {
        var today = await fx.Query<GetDailySummaryQuery, DailySummary>(new GetDailySummaryQuery(Tenant, fx.Api.Timeline.Today));
        var tables = await fx.Query<ListTablesQuery, IReadOnlyList<TableView>>(new ListTablesQuery(Tenant));
        var running = await fx.Query<ListRunningRatingsQuery, IReadOnlyList<RunningRatingView>>(new ListRunningRatingsQuery(Tenant, null));

        Assert.Equal(1, today.Cage.ShiftsOpen);
        Assert.Contains(tables, t => t.IsOpen);
        Assert.NotEmpty(running);
        Assert.Equal(fx.Api.Timeline.Now, fx.Api.Clock.UtcNow);
    }

    [Fact]
    public async Task KK5_data_without_completion_marker_is_refused_and_tenant_is_not_touched()
    {
        var other = new TenantId(Guid.NewGuid());
        await using (var scope = fx.Api.Services.CreateAsyncScope())
        {
            var register = scope.ServiceProvider.GetRequiredService<Sgt.BuildingBlocks.ICommandHandler<RegisterPlayerCommand, PlayerRegistered>>();
            var details = new PlayerDetails("Yarım", "Yükleme", new DateOnly(1980, 1, 1), "TR", DocumentType.NationalId, "DM99999999", null);
            await register.Handle(new RegisterPlayerCommand(other, details, new AuditContext(DemoUsers.Host.Id, "TEST", ReasonCode.Of("TEST"), Guid.NewGuid())), default);
        }

        var error = await Assert.ThrowsAsync<DemoSeedException>(() => fx.Generate(fx.Options with { Tenant = other }));

        Assert.Contains("reset önerilir", error.Message, StringComparison.Ordinal);
        Assert.Empty(await fx.Query<ListUsersQuery, IReadOnlyList<UserSummary>>(new ListUsersQuery(other)));
    }

    [Fact]
    public async Task Concurrent_run_for_the_same_tenant_is_refused()
    {
        var locks = fx.Api.Services.GetRequiredService<IAdvisoryLock>();
        await using var held = await locks.TryAcquire(DemoDataGenerator.LockName(Tenant), default);
        Assert.NotNull(held);

        var error = await Assert.ThrowsAsync<DemoSeedException>(() => fx.Generate());

        Assert.Contains("sürüyor", error.Message, StringComparison.Ordinal);
    }

    private Task<IReadOnlyList<PlayerSummary>> SearchPlayer(string number) =>
        fx.Query<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>(new SearchPlayersQuery(Tenant, number));
}
