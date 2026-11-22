using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Bir modülün outbox'ındaki bekleyen olayları dağıtır. Satırlar FOR UPDATE SKIP LOCKED ile alınır;
/// birden fazla sunucu aynı olayı iki kez almaz.
/// </summary>
public sealed class OutboxProcessor<TContext>(
    IServiceScopeFactory scopes,
    IntegrationEventSerializer serializer,
    IntegrationEventDispatcher dispatcher,
    IClock clock)
    where TContext : DbContext
{
    /// <summary>Ele alınan olay sayısı (başarılı + hatalı); arka plan servisi boşta beklemeye karar vermek için kullanır.</summary>
    public async Task<int> ProcessBatch(int batchSize, CancellationToken ct) => (await Process(batchSize, ct)).Total;

    /// <summary>Bir partiyi işler; dağıtılan ve hata alıp ertelenen olayları ayrı sayar.</summary>
    public async Task<OutboxBatchResult> Process(int batchSize, CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var schema = db.Model.GetDefaultSchema() ?? "public";
        if (!schema.All(c => char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c) || c == '_'))
        {
            throw new InvalidOperationException($"Geçersiz şema adı: {schema}");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Şema adı kod içindeki model tanımından gelir ve yukarıda doğrulanır; kullanıcı girdisi değildir.
#pragma warning disable EF1003
        var batch = await db.Set<OutboxMessage>()
            .FromSqlRaw(
                $"SELECT * FROM \"{schema}\".outbox_messages " +
                "WHERE processed_at IS NULL AND dead_lettered = false AND next_attempt_at <= {0} " +
                "ORDER BY occurred_at LIMIT {1} FOR UPDATE SKIP LOCKED",
                clock.UtcNow,
                batchSize)
            .ToListAsync(ct);
#pragma warning restore EF1003

        var failed = 0;
        foreach (var message in batch)
        {
            try
            {
                await dispatcher.Dispatch(serializer.FromOutbox(message), ct);
                message.MarkProcessed(clock.UtcNow);
            }
#pragma warning disable CA1031 // Tüketici hatası olayı kaybettirmemeli; kaydedilip yeniden denenir (KK4).
            catch (Exception e)
#pragma warning restore CA1031
            {
                message.MarkFailed($"{e.GetType().Name}: {e.Message}", clock.UtcNow);
                failed++;
            }
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new OutboxBatchResult(batch.Count - failed, failed);
    }
}

/// <summary>Bir partinin sonucu: dağıtılan ve hata alıp yeniden denemeye bırakılan olaylar.</summary>
public sealed record OutboxBatchResult(int Dispatched, int Failed)
{
    public int Total => Dispatched + Failed;
}
