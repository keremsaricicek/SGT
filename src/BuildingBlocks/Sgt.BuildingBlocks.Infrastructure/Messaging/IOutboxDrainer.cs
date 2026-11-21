using Microsoft.EntityFrameworkCore;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Bir modülün outbox'ını arka plan servisini beklemeden o an boşaltır (Spec 012 R3: demo verisi üretimi).
/// Modülden bağımsızdır; her <c>AddOutbox</c> çağrısı bir tane kaydeder.
/// </summary>
public interface IOutboxDrainer
{
    /// <summary>İşlenmeye hazır olay kalmayana kadar dağıtır; ele alınan olay sayısını döner.</summary>
    Task<int> Drain(CancellationToken ct);
}

/// <summary>
/// <see cref="OutboxProcessor{TContext}.ProcessBatch"/> 0 dönene kadar çağırır. Hata alan olay geri çekilme
/// süresi kadar ertelendiği için aynı anda tekrar seçilmez; döngü sonlanır.
/// </summary>
public sealed class OutboxDrainer<TContext>(OutboxProcessor<TContext> processor) : IOutboxDrainer
    where TContext : DbContext
{
    private const int BatchSize = 200;

    public async Task<int> Drain(CancellationToken ct)
    {
        var total = 0;
        int processed;
        while ((processed = await processor.ProcessBatch(BatchSize, ct)) > 0)
        {
            total += processed;
        }

        return total;
    }
}
