using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.DemoData;

/// <summary>
/// Tüm modüllerin outbox'larını boşaltır (Spec 012 R3). Bir olayın tüketicisi yeni olay yazabileceği için
/// (ör. rating → puan → denetim kaydı), hiçbir modülde işlenecek olay kalmayana kadar tur atar.
/// Boşaltma sürerken yeni olay yazılıyorsa onlar da alınır; döngü üretim durunca biter.
/// </summary>
public sealed class DemoOutbox(IEnumerable<IOutboxDrainer> drainers)
{
    public async Task<int> DrainAll(CancellationToken ct)
    {
        var total = 0;
        int processed;
        do
        {
            // Modül outbox'ları birbirinden bağımsızdır; aynı anda boşaltılır.
            processed = (await Task.WhenAll(drainers.Select(d => d.Drain(ct)))).Sum();
            total += processed;
        }
        while (processed > 0);

        return total;
    }
}
