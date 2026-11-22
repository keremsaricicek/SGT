using Sgt.BuildingBlocks;

namespace Sgt.Modules.Configuration.Contracts;

/// <summary>Tenant'a bir işaret kondu (ör. demo verisi tamamlandı); denetime düşer.</summary>
public sealed record TenantMarkerSetV1(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    TenantId TenantId,
    AuditContext Audit,
    string Code) : IAuditableEvent
{
    public string Summary => $"Tenant işareti: {Code}";
}
