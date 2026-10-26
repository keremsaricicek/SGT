using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

public enum PermissionChangeTarget
{
    Role,
    User,
}

public enum PermissionChangeKind
{
    RoleCreated,
    PermissionGranted,
    PermissionRevoked,
    PermissionDenied,
    PermissionUndenied,
    RoleAssigned,
    RoleUnassigned,
}

/// <summary>Silinemez yetki değişiklik kaydı: kim, kime, ne zaman, ne.</summary>
public sealed record PermissionChange(
    Guid Id,
    TenantId TenantId,
    PermissionChangeTarget Target,
    Guid TargetId,
    PermissionChangeKind Kind,
    string Item,
    AuditContext Audit,
    DateTimeOffset OccurredAt);
