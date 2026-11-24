using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Application.Slots;

/// <summary>
/// NEXIO adaptörü ve S Slot'tan gelen sayaçları kaydeder. Kural ihlali (bilinmeyen makine, geri giden sayaç)
/// olayı yeniden denetir ve sonunda operatörün dead-letter ekranına düşürür.
/// </summary>
public sealed class SlotMetersReportedHandler(MeterReadingRecorder recorder, IFloorInbox inbox) : IIntegrationEventHandler<SlotMetersReportedV1>
{
    private const string HandlerName = nameof(SlotMetersReportedHandler);

    /// <summary>Otomatik veri girişinin denetim kimliği: sistem kullanıcısı, terminal = kaynak.</summary>
    private static readonly UserId SystemActor = new(Guid.Empty);

    public async Task Handle(SlotMetersReportedV1 integrationEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        var e = integrationEvent;
        if (await inbox.AlreadyProcessed(e.EventId, HandlerName, ct))
        {
            return;
        }

        if (!Enum.TryParse<ReadingSource>(e.Source, ignoreCase: true, out var source) || source is ReadingSource.Manual or ReadingSource.Import)
        {
            throw new InvalidOperationException($"Geçersiz otomatik slot veri kaynağı: {e.Source}");
        }

        inbox.MarkProcessed(e.EventId, HandlerName);
        var result = await recorder.Record(
            e.TenantId,
            new MeterReadingInput(e.AssetNumber, e.ReadAt, new MeterSnapshot(e.CoinIn, e.CoinOut, e.Jackpot, e.GamesPlayed), source, e.Reference, e.IsReset, e.Note),
            new AuditContext(SystemActor, source.ToString(), ReasonCode.Of("AUTO_INGEST"), e.CorrelationId),
            ct);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"{e.AssetNumber} sayaç okuması reddedildi: {result.Error.Code}");
        }
    }
}
