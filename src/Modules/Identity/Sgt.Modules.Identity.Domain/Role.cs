using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

/// <summary>Yetki grubu. Role verilen yetki o roldeki herkese uygulanır.</summary>
public sealed class Role
{
    private readonly PermissionChangeLog _changes;

    private Role(RoleId id, TenantId tenantId, string name)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        Permissions = PermissionSet.Empty;
        _changes = new PermissionChangeLog(tenantId, PermissionChangeTarget.Role, id.Value);
    }

    public RoleId Id { get; }

    public TenantId TenantId { get; }

    public string Name { get; }

    public PermissionSet Permissions { get; private set; }

    public IReadOnlyList<PermissionChange> PendingChanges => _changes.Pending;

    public static Result<Role> Create(RoleId id, string name, AdminActor actor, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (string.IsNullOrWhiteSpace(name))
        {
            return IdentityErrors.RoleNameRequired;
        }

        var role = new Role(id, actor.TenantId, name.Trim());
        role._changes.Record(PermissionChangeKind.RoleCreated, role.Name, actor.Audit, at);
        return role;
    }

    /// <summary>Kalıcı kayıttan geri yükler; değişiklik kaydı üretmez. Yalnızca altyapı kullanır.</summary>
    public static Role Restore(RoleId id, TenantId tenantId, string name, PermissionSet permissions) =>
        new(id, tenantId, name) { Permissions = permissions };

    public Result Grant(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsFailure || Permissions.Contains(code))
        {
            return allowed; // aynı yetkiyi tekrar vermek kayıt üretmez (KK7)
        }

        var updated = Permissions.With(code);
        if (updated.IsFailure)
        {
            return updated.Error;
        }

        Apply(updated.Value, PermissionChangeKind.PermissionGranted, code, actor.Audit, at);
        return Result.Success();
    }

    public Result Revoke(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsFailure || !Permissions.Contains(code))
        {
            return allowed; // olmayan yetkiyi geri almak kayıt üretmez (KK7)
        }

        var updated = Permissions.Without(code);
        if (updated.IsFailure)
        {
            return updated.Error;
        }

        Apply(updated.Value, PermissionChangeKind.PermissionRevoked, code, actor.Audit, at);
        return Result.Success();
    }

    public void ClearPendingChanges() => _changes.Clear();

    private Result EnsureSameTenant(AdminActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return actor.TenantId == TenantId ? Result.Success() : IdentityErrors.TenantMismatch;
    }

    private void Apply(PermissionSet updated, PermissionChangeKind kind, PermissionCode code, AuditContext audit, DateTimeOffset at)
    {
        Permissions = updated;
        _changes.Record(kind, code.ToString(), audit, at);
    }
}
