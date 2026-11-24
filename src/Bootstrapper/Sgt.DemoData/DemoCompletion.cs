using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Markers;

namespace Sgt.DemoData;

/// <summary>
/// Yüklemenin bitişi: saat gerçek ana gelir, tüm outbox'lar boşaltılır ve boş olduğu doğrulanır, sonra tamamlanma işareti
/// konur (son yazım). İşaretin kendi denetim olayı da dağıtılır.
/// </summary>
public sealed class DemoCompletion(DemoOutbox outbox, DemoCommands commands, DemoClock clock)
{
    /// <summary>Demo verisinin tamamı yazıldı.</summary>
    public const string CompletedMarker = "DEMO_DATA_V1";

    public async Task Complete(DemoSeedOptions options, AuditContext audit, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (clock.UtcNow < options.Timeline.Now)
        {
            clock.Set(options.Timeline.Now);
        }

        await outbox.DrainAll(ct);
        await outbox.EnsureEmpty(options.Tenant, ct);
        await commands.Do<ICommandHandler<SetTenantMarkerCommand>>(
            "tamamlanma işareti", h => h.Handle(new SetTenantMarkerCommand(options.Tenant, CompletedMarker, audit), ct));

        try
        {
            await outbox.DrainAll(ct);
            await outbox.EnsureEmpty(options.Tenant, ct);
        }
        catch (DemoSeedException e)
        {
            throw new DemoSeedException(
                "Demo verisi tamamlandı ve işaret yazıldı, ancak işaretin denetim olayı henüz dağıtılamadı; olay bekliyor ve " +
                "API çalışırken arka plan servisi tarafından dağıtılacak. Ayrıntı: " + e.Message);
        }
    }
}
