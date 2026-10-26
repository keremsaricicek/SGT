using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.ExchangeRates;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Modules.Configuration.Tests;

public sealed class ExchangeRateTests
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    private static readonly DateTimeOffset T0 = new(2026, 10, 26, 9, 0, 0, TimeSpan.Zero);
    private static readonly AuditContext Audit = new(UserId.New(), "KASA-1", ReasonCode.Of("DAILY_RATE"), Guid.NewGuid());

    private readonly FakeRates _rates = new();
    private readonly FakeOutbox _outbox = new();
    private readonly ManualClock _clock = new() { UtcNow = T0 };

    [Theory]
    [InlineData("JPY", 1, 2)]
    [InlineData("TRY", 1, 1)]
    [InlineData("EUR", 0, 1)]
    [InlineData("EUR", 36, 35)]
    [InlineData("EUR", 36.1234567, 37)]
    public void KK1_KK2_invalid_rates_are_rejected(string currency, decimal buy, decimal sell)
    {
        Assert.True(ExchangeRate.Set(Tenant, currency, buy, sell, Audit, T0).IsFailure);
    }

    [Fact]
    public async Task KK2_KK5_setting_a_rate_stores_it_and_publishes_auditable_event()
    {
        var result = await SetRate("eur", 36.5m, 37.25m);

        Assert.True(result.IsSuccess);
        var rate = Assert.Single(_rates.Items);
        Assert.Equal("EUR", rate.Currency);
        var published = Assert.IsType<ExchangeRateSetV1>(Assert.Single(_outbox.Events));
        Assert.Equal(rate.Id, published.EventId);
        Assert.Equal(Audit, published.Audit);
        Assert.Contains("EUR", published.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task KK4_rate_at_a_moment_is_the_latest_one_entered_before_it()
    {
        await SetRate("USD", 32m, 33m);
        _clock.UtcNow = T0.AddHours(3);
        await SetRate("USD", 32.5m, 33.5m);

        var query = new ExchangeRateQuery(_rates);

        Assert.Null(await query.RateAt(Tenant, "USD", T0.AddMinutes(-1), default));
        Assert.Equal(33m, (await query.RateAt(Tenant, "USD", T0.AddHours(1), default))!.Sell);
        Assert.Equal(33.5m, (await query.RateAt(Tenant, "usd", T0.AddHours(4), default))!.Sell);
        Assert.Null(await query.RateAt(new TenantId(Guid.NewGuid()), "USD", T0.AddHours(4), default));
    }

    [Fact]
    public async Task KK7_current_rates_lists_latest_rate_per_currency()
    {
        await SetRate("USD", 32m, 33m);
        await SetRate("EUR", 35m, 36m);
        _clock.UtcNow = T0.AddHours(1);
        await SetRate("USD", 31m, 32m);

        var current = await new GetCurrentRatesHandler(_rates, _clock).Handle(new GetCurrentRatesQuery(Tenant), default);

        Assert.Equal(["EUR", "USD"], current.Select(r => r.Currency));
        Assert.Equal(31m, current[1].Buy);
    }

    private Task<Result> SetRate(string currency, decimal buy, decimal sell) =>
        new SetExchangeRateHandler(_rates, _outbox, new NoopUnitOfWork(), _clock)
            .Handle(new SetExchangeRateCommand(Tenant, currency, buy, sell, Audit), default);

    private sealed class FakeRates : IExchangeRateStore
    {
        public List<ExchangeRate> Items { get; } = [];

        public void Add(ExchangeRate rate) => Items.Add(rate);

        public Task<ExchangeRate?> LatestBefore(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct) =>
            Task.FromResult(Items
                .Where(r => r.TenantId == tenantId && r.Currency == currency && r.EffectiveFrom <= at)
                .MaxBy(r => r.EffectiveFrom));
    }

    private sealed class FakeOutbox : IConfigurationOutbox
    {
        public List<IIntegrationEvent> Events { get; } = [];

        public void Add(IIntegrationEvent integrationEvent) => Events.Add(integrationEvent);
    }

    private sealed class NoopUnitOfWork : IConfigurationUnitOfWork
    {
        public Task Commit(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class ManualClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }
}
