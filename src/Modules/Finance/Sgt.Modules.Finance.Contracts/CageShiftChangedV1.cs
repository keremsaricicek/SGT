using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Contracts;

/// <summary>Kasa vardiyasında açılış, işlem veya kapanış oldu (Spec 006 KK11). Denetime düşer.</summary>
public sealed record CageShiftChangedV1(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    TenantId TenantId,
    AuditContext Audit,
    Guid ShiftId,
    Guid? TransactionId,
    string Kind,
    string Detail) : IAuditableEvent
{
    public string Summary => $"Kasa vardiyası {ShiftId}: {Kind} {Detail}".TrimEnd();
}
