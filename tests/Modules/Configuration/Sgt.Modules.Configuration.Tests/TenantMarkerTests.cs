using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Markers;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Modules.Configuration.Tests;

/// <summary>Tenant işareti: bir kurulum adımının (ör. demo verisi) tamamlandığını kalıcı ve denetimli kaydeder.</summary>
public sealed class TenantMarkerTests
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    private static readonly DateTimeOffset T0 = new(2026, 11, 22, 9, 0, 0, TimeSpan.Zero);
    private static readonly AuditContext Audit = new(UserId.New(), "DEMO-SEED", ReasonCode.Of("DEMO"), Guid.NewGuid());

    private readonly FakeMarkers _markers = new();
    private readonly FakeOutbox _outbox = new();

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("demo data")]
    [InlineData("DEMO-DATA")]
    public void Invalid_codes_are_rejected(string code)
    {
        Assert.Equal(ConfigurationErrors.InvalidMarkerCode, TenantMarker.Set(Tenant, code, Audit, T0).Error);
    }

    [Fact]
    public async Task Setting_a_marker_stores_it_and_publishes_auditable_event()
    {
        var result = await Set("demo_data_v1");

        Assert.True(result.IsSuccess);
        var marker = Assert.Single(_markers.Items);
        Assert.Equal("DEMO_DATA_V1", marker.Code);
        Assert.Equal(T0, marker.SetAt);
        var published = Assert.IsType<TenantMarkerSetV1>(Assert.Single(_outbox.Events));
        Assert.Equal(Audit, published.Audit);
        Assert.Contains("DEMO_DATA_V1", published.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Same_marker_cannot_be_set_twice()
    {
        await Set("DEMO_DATA_V1");

        var second = await Set("DEMO_DATA_V1");

        Assert.Equal(ConfigurationErrors.MarkerAlreadySet, second.Error);
        Assert.Single(_markers.Items);
    }

    [Fact]
    public async Task Query_reports_marker_per_tenant()
    {
        await Set("DEMO_DATA_V1");
        var handler = new IsTenantMarkerSetHandler(_markers);

        Assert.True(await handler.Handle(new IsTenantMarkerSetQuery(Tenant, "demo_data_v1"), default));
        Assert.False(await handler.Handle(new IsTenantMarkerSetQuery(new TenantId(Guid.NewGuid()), "DEMO_DATA_V1"), default));
        Assert.False(await handler.Handle(new IsTenantMarkerSetQuery(Tenant, "OTHER"), default));
    }

    private Task<Result> Set(string code) =>
        new SetTenantMarkerHandler(_markers, _outbox, new NoopUnitOfWork(), new FixedClock())
            .Handle(new SetTenantMarkerCommand(Tenant, code, Audit), default);

    private sealed class FakeMarkers : ITenantMarkerStore
    {
        public List<TenantMarker> Items { get; } = [];

        public void Add(TenantMarker marker) => Items.Add(marker);

        public Task<bool> Exists(TenantId tenantId, string code, CancellationToken ct) =>
            Task.FromResult(Items.Any(m => m.TenantId == tenantId && m.Code == code));
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

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => T0;
    }
}
