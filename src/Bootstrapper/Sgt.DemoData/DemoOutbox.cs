using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.DemoData;

/// <summary>
/// Tüm modüllerin outbox'larını boşaltır (Spec 012 R3). Bir olayın tüketicisi yeni olay yazabileceği için
/// (ör. rating → puan → denetim kaydı), hiçbir modülde dağıtılacak olay kalmayana kadar tur atar.
/// Boşaltma sürerken yeni olay yazılıyorsa onlar da alınır; döngü üretim durunca biter.
/// </summary>
public sealed class DemoOutbox(OutboxDrainers drainers)
{
    public async Task DrainAll(CancellationToken ct)
    {
        int dispatched;
        do
        {
            // Modül outbox'ları birbirinden bağımsızdır; aynı anda boşaltılır.
            dispatched = (await Task.WhenAll(drainers.All.Select(d => d.Drain(ct)))).Sum(r => r.Dispatched);
        }
        while (dispatched > 0);
    }

    /// <summary>Tenant'ın hiçbir modülünde işlenmemiş veya dead-letter olay kalmadığını doğrular.</summary>
    public async Task EnsureEmpty(TenantId tenantId, CancellationToken ct)
    {
        var problems = new List<string>();
        foreach (var drainer in drainers.All)
        {
            var backlog = await drainer.Backlog(tenantId, ct);
            if (!backlog.IsEmpty)
            {
                problems.Add($"{drainer.Module}: {backlog.Pending} bekleyen, {backlog.DeadLettered} dead-letter");
            }
        }

        if (problems.Count > 0)
        {
            throw new DemoSeedException("Olaylar işlenemedi (" + string.Join("; ", problems) + "). Veritabanını sıfırlayın: build/demo.sh reset --yes");
        }
    }
}
