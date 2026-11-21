using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed class OutboxDrainerTests(MessagingFixture fx) : IClassFixture<MessagingFixture>
{
    [Fact]
    public async Task Drain_dispatches_every_pending_event_across_batches_and_reports_count()
    {
        var events = Enumerable.Range(0, 120).Select(_ => NewEvent()).ToList();
        await using (var scope = fx.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var serializer = fx.Services.GetRequiredService<IntegrationEventSerializer>();
            db.Set<OutboxMessage>().AddRange(events.Select(serializer.ToOutbox));
            await db.SaveChangesAsync();
        }

        var drainer = new OutboxDrainer<TestDbContext>(fx.Services.GetRequiredService<OutboxProcessor<TestDbContext>>());
        var drained = await drainer.Drain(default);

        Assert.Equal(120, drained);
        Assert.All(events, e => Assert.Contains(e.EventId, fx.Handler.Received));
        await using var check = fx.Services.CreateAsyncScope();
        Assert.False(await check.ServiceProvider.GetRequiredService<TestDbContext>().Set<OutboxMessage>().AnyAsync(m => m.ProcessedAt == null));
        Assert.Equal(0, await drainer.Drain(default));
    }

    [Fact]
    public void AddOutbox_registers_a_drainer_per_module()
    {
        var services = new ServiceCollection();

        services.AddOutbox<TestDbContext>("A");

        Assert.Contains(services, d => d.ServiceType == typeof(IOutboxDrainer) && d.ImplementationType == typeof(OutboxDrainer<TestDbContext>));
    }

    private SampleHappenedV1 NewEvent() => new(
        Guid.NewGuid(), Guid.NewGuid(), fx.Clock.UtcNow, new TenantId(Guid.NewGuid()),
        new AuditContext(UserId.New(), "PC-1", ReasonCode.Of("TEST"), Guid.NewGuid()), "boşaltma");
}
