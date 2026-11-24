namespace Sgt.Modules.Identity.Infrastructure.Persistence;

// Veritabanı satırları. Yalnızca kalıcılık içindir; iş kuralı domain nesnelerindedir.

internal sealed class RoleRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];

    /// <summary>PostgreSQL xmin; eşzamanlı değişikliği yakalar.</summary>
    public uint Version { get; set; }
}

internal sealed class UserAccessRow
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public bool IsSuperAdmin { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string EmployeeCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<Guid> RoleIds { get; set; } = [];

    public List<string> Grants { get; set; } = [];

    public List<string> Denied { get; set; } = [];

    /// <summary>PostgreSQL xmin; eşzamanlı değişikliği yakalar.</summary>
    public uint Version { get; set; }
}

/// <summary>Append-only: veritabanı tetikleyicisi UPDATE ve DELETE'i reddeder.</summary>
internal sealed class PermissionChangeRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Target { get; set; } = string.Empty;

    public Guid TargetId { get; set; }

    public string Kind { get; set; } = string.Empty;

    public string Item { get; set; } = string.Empty;

    public Guid ActorId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
