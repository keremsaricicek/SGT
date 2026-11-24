using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Contracts;

/// <summary>
/// Bir rolün veya kullanıcının yetkisi değişti (Spec 001 KK6, Spec 002 KK9).
/// Target: "Role" | "User"; Kind: RoleCreated, PermissionGranted, ... ; Item: yetki kodu, rol adı veya rol kimliği.
/// </summary>
public sealed record PermissionChangedV1(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    TenantId TenantId,
    AuditContext Audit,
    string Target,
    Guid TargetId,
    string Kind,
    string Item) : IAuditableEvent
{
    public string Summary => $"{Target} {TargetId}: {Kind} {Item}";
}
