using Sgt.BuildingBlocks;
using Sgt.Modules.Audit.Application.Ports;
using Sgt.Modules.Audit.Domain;

namespace Sgt.Modules.Audit.Application.Recording;

/// <summary>Denetime tabi her olayı bir denetim kaydına çevirir (KK6). Aynı olay iki kez kaydedilmez (KK3).</summary>
public sealed class RecordAuditEntryHandler(
    IAuditLog log,
    IAuditInbox inbox,
    IAuditUnitOfWork unitOfWork,
    IClock clock) : IIntegrationEventHandler<IAuditableEvent>
{
    private const string HandlerName = nameof(RecordAuditEntryHandler);

    public async Task Handle(IAuditableEvent integrationEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        if (await inbox.AlreadyProcessed(integrationEvent.EventId, HandlerName, ct))
        {
            return;
        }

        log.Add(AuditEntry.From(integrationEvent, clock.UtcNow));
        inbox.MarkProcessed(integrationEvent.EventId, HandlerName);
        await unitOfWork.Commit(ct);
    }
}
