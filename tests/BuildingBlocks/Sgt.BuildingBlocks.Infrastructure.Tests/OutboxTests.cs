using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed class OutboxTests(MessagingFixture fx) : IClassFixture<MessagingFixture>
{
    [Fact]
    public async Task KK1_event_is_not_stored_when_transaction_rolls_back()
    {
        var e = NewEvent();
        await using (var scope = fx.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            await using var tx = await db.Database.BeginTransactionAsync();
            db.Set<OutboxMessage>().Add(Serializer().ToOutbox(e));
            await db.SaveChangesAsync();
            await tx.RollbackAsync();
        }

        Assert.Null(await Find(e.EventId));
    }

    [Fact]
    public async Task KK2_pending_event_is_dispatched_to_type_and_interface_subscribers_and_marked_processed()
    {
        var e = await Store(NewEvent());

        await ProcessAll();

        Assert.Contains(e.EventId, fx.Handler.Received);
        Assert.Contains(e.EventId, fx.Handler.ReceivedAsAuditable);
        Assert.NotNull((await Find(e.EventId))!.ProcessedAt);
    }

    [Fact]
    public async Task KK4_failure_backs_off_then_dead_letters_after_max_attempts_and_KK5_operator_reprocesses()
    {
        var e = await Store(NewEvent());
        fx.Handler.Fail = true;
        try
        {
            await ProcessAll();
            var first = (await Find(e.EventId))!;
            Assert.Equal(1, first.Attempts);
            Assert.Equal(fx.Clock.UtcNow.AddSeconds(2), first.NextAttemptAt);

            for (var i = 1; i < OutboxMessage.MaxAttempts; i++)
            {
                fx.Clock.UtcNow = fx.Clock.UtcNow.AddHours(2);
                await ProcessAll();
            }

            var dead = (await Find(e.EventId))!;
            Assert.True(dead.DeadLettered);
            Assert.Contains("tüketici hatası", dead.LastError, StringComparison.Ordinal);

            await using var scope = fx.Services.CreateAsyncScope();
            var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            Assert.Contains(await store.DeadLetters(10, default), d => d.EventId == e.EventId);
            Assert.True(await store.Reprocess(e.EventId, default));
        }
        finally
        {
            fx.Handler.Fail = false;
        }

        await ProcessAll();
        Assert.Contains(e.EventId, fx.Handler.Received);
    }

    [Fact]
    public void Serializer_round_trips_value_objects()
    {
        var e = NewEvent();

        var back = (SampleHappenedV1)Serializer().FromOutbox(Serializer().ToOutbox(e));

        Assert.Equal(e, back);
    }

    [Fact]
    public void Serializer_rejects_unregistered_event_types()
    {
        Assert.Throws<InvalidOperationException>(() => new IntegrationEventSerializer([]).ToOutbox(NewEvent()));
    }

    [Fact]
    public async Task KK3_inbox_remembers_processed_events()
    {
        var eventId = Guid.NewGuid();
        await using (var scope = fx.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var inbox = new EfInbox<TestDbContext>(db, fx.Clock);
            Assert.False(await inbox.AlreadyProcessed(eventId, "H", default));
            inbox.MarkProcessed(eventId, "H");
            await db.SaveChangesAsync();
        }

        await using var check = fx.Services.CreateAsyncScope();
        var checkInbox = new EfInbox<TestDbContext>(check.ServiceProvider.GetRequiredService<TestDbContext>(), fx.Clock);
        Assert.True(await checkInbox.AlreadyProcessed(eventId, "H", default));
        Assert.False(await checkInbox.AlreadyProcessed(eventId, "Other", default));
    }

    private SampleHappenedV1 NewEvent() => new(
        Guid.NewGuid(), Guid.NewGuid(), fx.Clock.UtcNow, new TenantId(Guid.NewGuid()),
        new AuditContext(UserId.New(), "PC-1", ReasonCode.Of("TEST"), Guid.NewGuid()), "örnek");

    private IntegrationEventSerializer Serializer() => fx.Services.GetRequiredService<IntegrationEventSerializer>();

    private async Task<SampleHappenedV1> Store(SampleHappenedV1 e)
    {
        await using var scope = fx.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        db.Set<OutboxMessage>().Add(Serializer().ToOutbox(e));
        await db.SaveChangesAsync();
        return e;
    }

    private async Task ProcessAll()
    {
        var processor = fx.Services.GetRequiredService<OutboxProcessor<TestDbContext>>();
        while (await processor.ProcessBatch(50, default) > 0)
        {
            if (fx.Handler.Fail)
            {
                break;
            }
        }
    }

    private async Task<OutboxMessage?> Find(Guid id)
    {
        await using var scope = fx.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<TestDbContext>().Set<OutboxMessage>().AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == id);
    }
}
