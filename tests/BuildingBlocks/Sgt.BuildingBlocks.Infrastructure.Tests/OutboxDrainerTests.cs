using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed class OutboxDrainerTests(MessagingFixture fx) : IClassFixture<MessagingFixture>
{
    [Fact]
    public async Task Drain_dispatches_every_pending_event_across_batches_and_leaves_no_backlog()
    {
        var tenant = new TenantId(Guid.NewGuid());
        var events = await Store(tenant, 450);
        var drainer = Drainer();

        var result = await drainer.Drain(default);

        Assert.Equal(new OutboxDrainResult(450, 0), result);
        Assert.All(events, e => Assert.Contains(e.EventId, fx.Handler.Received));
        Assert.Equal(new OutboxBacklog(0, 0), await drainer.Backlog(tenant, default));
        Assert.Equal(new OutboxDrainResult(0, 0), await drainer.Drain(default));
    }

    [Fact]
    public async Task Failed_events_are_reported_as_failed_not_processed_and_stay_in_backlog()
    {
        var tenant = new TenantId(Guid.NewGuid());
        await Store(tenant, 3);
        var drainer = Drainer();
        fx.Handler.Fail = true;
        try
        {
            var result = await drainer.Drain(default);

            Assert.Equal(new OutboxDrainResult(0, 3), result);
            Assert.Equal(new OutboxBacklog(3, 0), await drainer.Backlog(tenant, default));
        }
        finally
        {
            fx.Handler.Fail = false;
        }

        fx.Clock.UtcNow = fx.Clock.UtcNow.AddMinutes(5);
        Assert.Equal(new OutboxDrainResult(3, 0), await drainer.Drain(default));
        Assert.Equal(new OutboxBacklog(0, 0), await drainer.Backlog(tenant, default));
    }

    [Fact]
    public void AddOutbox_does_not_expose_drainers_in_production_registrations()
    {
        var services = new ServiceCollection();

        services.AddOutbox<TestDbContext>("A");

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IOutboxDrainer) || d.ServiceType == typeof(OutboxDrainers));
    }

    [Fact]
    public void AddOutboxDrainers_creates_one_drainer_per_registered_outbox()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock>(fx.Clock);
        OutboxDrainerRegistration.AddOutboxDrainers(services);
        services.AddOutbox<TestDbContext>("A");

        using var provider = services.BuildServiceProvider();
        var drainer = Assert.Single(provider.GetRequiredService<OutboxDrainers>().All);
        Assert.Equal("A", drainer.Module);
    }

    private OutboxDrainer<TestDbContext> Drainer() =>
        new("Test", fx.Services.GetRequiredService<OutboxProcessor<TestDbContext>>(), fx.Services.GetRequiredService<IServiceScopeFactory>());

    private async Task<List<SampleHappenedV1>> Store(TenantId tenant, int count)
    {
        var events = Enumerable.Range(0, count).Select(_ => new SampleHappenedV1(
            Guid.NewGuid(), Guid.NewGuid(), fx.Clock.UtcNow, tenant,
            new AuditContext(UserId.New(), "PC-1", ReasonCode.Of("TEST"), Guid.NewGuid()), "boşaltma")).ToList();
        await using var scope = fx.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var serializer = fx.Services.GetRequiredService<IntegrationEventSerializer>();
        db.Set<OutboxMessage>().AddRange(events.Select(serializer.ToOutbox));
        await db.SaveChangesAsync();
        return events;
    }
}
