using System.Collections.Immutable;
using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

/// <summary>
/// Bir kullanıcının yetki tanımı: atanmış roller, kişiye özel eklenen ve çıkarılan yetkiler.
/// Etkin yetki = (rollerin yetkileri ∪ eklenenler) − çıkarılanlar (KK3).
/// </summary>
public sealed class UserAccess
{
    private readonly HashSet<RoleId> _roleIds = [];
    private readonly HashSet<PermissionCode> _denied = [];
    private readonly PermissionChangeLog _changes;

    private UserAccess(UserId userId, TenantId tenantId, bool isSuperAdmin)
    {
        UserId = userId;
        TenantId = tenantId;
        IsSuperAdmin = isSuperAdmin;
        Grants = PermissionSet.Empty;
        _changes = new PermissionChangeLog(tenantId, PermissionChangeTarget.User, userId.Value);
    }

    public UserId UserId { get; }

    public TenantId TenantId { get; }

    public bool IsSuperAdmin { get; }

    public IReadOnlySet<RoleId> RoleIds => _roleIds;

    /// <summary>Rolden bağımsız, kişiye özel eklenen yetkiler.</summary>
    public PermissionSet Grants { get; private set; }

    /// <summary>Kişiden özel olarak çıkarılan yetkiler.</summary>
    public IReadOnlySet<PermissionCode> Denied => _denied;

    public IReadOnlyList<PermissionChange> PendingChanges => _changes.Pending;

    public static UserAccess Create(UserId userId, TenantId tenantId, bool isSuperAdmin) =>
        new(userId, tenantId, isSuperAdmin);

    public Result AssignRole(Role role, AdminActor actor, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(role);
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        if (role.TenantId != TenantId)
        {
            return IdentityErrors.TenantMismatch;
        }

        if (_roleIds.Add(role.Id))
        {
            _changes.Record(PermissionChangeKind.RoleAssigned, role.Id.Value.ToString(), actor, at);
        }

        return Result.Success();
    }

    public Result UnassignRole(RoleId roleId, AdminActor actor, DateTimeOffset at)
    {
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsSuccess && _roleIds.Remove(roleId))
        {
            _changes.Record(PermissionChangeKind.RoleUnassigned, roleId.Value.ToString(), actor, at);
        }

        return allowed;
    }

    public Result Grant(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsFailure || Grants.Contains(code))
        {
            return allowed;
        }

        var updated = Grants.With(code);
        if (updated.IsFailure)
        {
            return updated.Error;
        }

        Grants = updated.Value;
        _changes.Record(PermissionChangeKind.PermissionGranted, code.ToString(), actor, at);
        return Result.Success();
    }

    public Result Revoke(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsFailure || !Grants.Contains(code))
        {
            return allowed;
        }

        var updated = Grants.Without(code);
        if (updated.IsFailure)
        {
            return updated.Error;
        }

        Grants = updated.Value;
        _changes.Record(PermissionChangeKind.PermissionRevoked, code.ToString(), actor, at);
        return Result.Success();
    }

    /// <summary>Yetkiyi kişiden çıkarır; rolünde olsa bile etkisiz olur.</summary>
    public Result Deny(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(code);
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsSuccess && _denied.Add(code))
        {
            _changes.Record(PermissionChangeKind.PermissionDenied, code.ToString(), actor, at);
        }

        return allowed;
    }

    public Result Undeny(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(code);
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsSuccess && _denied.Remove(code))
        {
            _changes.Record(PermissionChangeKind.PermissionUndenied, code.ToString(), actor, at);
        }

        return allowed;
    }

    /// <summary>
    /// Etkin yetkileri hesaplar. Yalnızca bu kullanıcıya atanmış ve aynı tenant'taki roller dikkate alınır.
    /// Görme yetkisi olmayan ekranın diğer yetkileri etkisizdir.
    /// </summary>
    public IReadOnlySet<PermissionCode> EffectivePermissions(IEnumerable<Role> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        var granted = roles
            .Where(r => r.TenantId == TenantId && _roleIds.Contains(r.Id))
            .SelectMany(r => r.Permissions.Codes)
            .Concat(Grants.Codes)
            .Where(c => !_denied.Contains(c))
            .ToImmutableHashSet();

        return granted.Where(c => c.IsView || granted.Contains(c.ViewOfScreen())).ToImmutableHashSet();
    }

    public void ClearPendingChanges() => _changes.Clear();

    private Result EnsureSameTenant(AdminActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return actor.TenantId == TenantId ? Result.Success() : IdentityErrors.TenantMismatch;
    }
}
