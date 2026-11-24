using Sgt.BuildingBlocks;

namespace Sgt.Modules.Audit.Domain;

/// <summary>
/// Değiştirilemez denetim kaydı: kim, hangi terminalden, neden, ne yaptı, ne zaman.
/// Kimliği olayın kimliğidir; aynı olay iki kayıt üretemez.
/// </summary>
public sealed class AuditEntry
{
    private AuditEntry(
        Guid id, TenantId tenantId, string eventType, string summary, AuditContext audit, DateTimeOffset occurredAt, DateTimeOffset recordedAt)
    {
        Id = id;
        TenantId = tenantId;
        EventType = eventType;
        Summary = summary;
        Audit = audit;
        OccurredAt = occurredAt;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public string EventType { get; }

    public string Summary { get; }

    public AuditContext Audit { get; }

    public DateTimeOffset OccurredAt { get; }

    public DateTimeOffset RecordedAt { get; }

    public static AuditEntry From(IAuditableEvent auditableEvent, DateTimeOffset recordedAt)
    {
        ArgumentNullException.ThrowIfNull(auditableEvent);
        return new AuditEntry(
            auditableEvent.EventId,
            auditableEvent.TenantId,
            auditableEvent.GetType().FullName!,
            auditableEvent.Summary,
            auditableEvent.Audit,
            auditableEvent.OccurredAt,
            recordedAt);
    }

    /// <summary>Kalıcı kayıttan geri yükler. Yalnızca altyapı kullanır.</summary>
    public static AuditEntry Restore(
        Guid id, TenantId tenantId, string eventType, string summary, AuditContext audit, DateTimeOffset occurredAt, DateTimeOffset recordedAt) =>
        new(id, tenantId, eventType, summary, audit, occurredAt, recordedAt);
}
