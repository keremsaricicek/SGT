using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Boşaltma sonucu: dağıtılan olaylar ve hata alıp geri çekilme süresine bırakılan olaylar.</summary>
public sealed record OutboxDrainResult(int Dispatched, int Failed);

/// <summary>Bir tenant'ın outbox'ta kalan olayları: henüz işlenmemiş (hatalı olanlar dahil) ve dead-letter'a düşmüş.</summary>
public sealed record OutboxBacklog(int Pending, int DeadLettered)
{
    public bool IsEmpty => Pending == 0 && DeadLettered == 0;
}

/// <summary>
/// Bir modülün outbox'ını arka plan servisini beklemeden o an boşaltır (Spec 012 R3: demo verisi üretimi).
/// Üretim DI'ında kayıtlı değildir; yalnızca <see cref="MessagingServiceCollectionExtensions.AddOutboxDrainers"/> ile açılır.
/// </summary>
public interface IOutboxDrainer
{
    string Module { get; }

    /// <summary>İşlenmeye hazır olay kalmayana kadar dağıtır.</summary>
    Task<OutboxDrainResult> Drain(CancellationToken ct);

    Task<OutboxBacklog> Backlog(TenantId tenantId, CancellationToken ct);
}

/// <summary>Kayıtlı tüm modül outbox'larının boşaltıcıları.</summary>
public sealed class OutboxDrainers(IEnumerable<IOutboxDrainer> drainers) : List<IOutboxDrainer>(drainers);

/// <summary>
/// <see cref="OutboxProcessor{TContext}.Process"/> olay kalmayana kadar çağırır. Hata alan olay geri çekilme süresi kadar
/// ertelendiği için aynı anda tekrar seçilmez; döngü sonlanır ve olay <see cref="Backlog"/>'da görünür.
/// </summary>
public sealed class OutboxDrainer<TContext>(string module, OutboxProcessor<TContext> processor, IServiceScopeFactory scopes) : IOutboxDrainer
    where TContext : DbContext
{
    private const int BatchSize = 200;

    public string Module => module;

    public async Task<OutboxDrainResult> Drain(CancellationToken ct)
    {
        var dispatched = 0;
        var failed = 0;
        OutboxBatchResult batch;
        do
        {
            batch = await processor.Process(BatchSize, ct);
            dispatched += batch.Dispatched;
            failed += batch.Failed;
        }
        while (batch.Total > 0);

        return new OutboxDrainResult(dispatched, failed);
    }

    public async Task<OutboxBacklog> Backlog(TenantId tenantId, CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var messages = scope.ServiceProvider.GetRequiredService<TContext>().Set<OutboxMessage>().AsNoTracking()
            .Where(m => m.TenantId == tenantId.Value && m.ProcessedAt == null);
        return new OutboxBacklog(
            await messages.CountAsync(m => !m.DeadLettered, ct),
            await messages.CountAsync(m => m.DeadLettered, ct));
    }
}
