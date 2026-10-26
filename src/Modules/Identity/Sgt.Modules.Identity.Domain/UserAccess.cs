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

    private UserAccess(UserId userId, TenantId tenantId, bool isSuperAdmin, string displayName, string employeeCode, bool isActive)
    {
        UserId = userId;
        TenantId = tenantId;
        IsSuperAdmin = isSuperAdmin;
        DisplayName = displayName;
        EmployeeCode = employeeCode;
        IsActive = isActive;
        Grants = PermissionSet.Empty;
        _changes = new PermissionChangeLog(tenantId, PermissionChangeTarget.User, userId.Value);
    }

    public UserId UserId { get; }

    public TenantId TenantId { get; }

    public bool IsSuperAdmin { get; }

    public string DisplayName { get; }

    /// <summary>İşletmenin verdiği personel kodu; tenant içinde benzersizdir.</summary>
    public string EmployeeCode { get; }

    /// <summary>Pasif kullanıcının hiçbir yetkisi yoktur (Spec 003 KK4).</summary>
    public bool IsActive { get; private set; }

    public IReadOnlySet<RoleId> RoleIds => _roleIds;

    /// <summary>Rolden bağımsız, kişiye özel eklenen yetkiler.</summary>
    public PermissionSet Grants { get; private set; }

    /// <summary>Kişiden özel olarak çıkarılan yetkiler.</summary>
    public IReadOnlySet<PermissionCode> Denied => _denied;

    public IReadOnlyList<PermissionChange> PendingChanges => _changes.Pending;

    /// <summary>Sistem kurulumu için (ilk süper admin, test). Personel kaydı için <see cref="Register"/> kullanılır.</summary>
    public static UserAccess Create(UserId userId, TenantId tenantId, bool isSuperAdmin) =>
        new(userId, tenantId, isSuperAdmin, string.Empty, string.Empty, isActive: true);

    /// <summary>Yeni personel kaydı (Spec 003 KK1).</summary>
    public static Result<UserAccess> Register(
        UserId userId, TenantId tenantId, string displayName, string employeeCode, AuditContext audit, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return IdentityErrors.DisplayNameRequired;
        }

        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return IdentityErrors.EmployeeCodeRequired;
        }

        var user = new UserAccess(userId, tenantId, isSuperAdmin: false, displayName.Trim(), employeeCode.Trim().ToUpperInvariant(), isActive: true);
        user._changes.Record(PermissionChangeKind.UserCreated, $"{user.EmployeeCode} {user.DisplayName}", audit, at);
        return user;
    }

    /// <summary>Kalıcı kayıttan geri yükler; değişiklik kaydı üretmez. Yalnızca altyapı kullanır.</summary>
    public static UserAccess Restore(
        UserId userId,
        TenantId tenantId,
        bool isSuperAdmin,
        string displayName,
        string employeeCode,
        bool isActive,
        IEnumerable<RoleId> roleIds,
        PermissionSet grants,
        IEnumerable<PermissionCode> denied)
    {
        var user = new UserAccess(userId, tenantId, isSuperAdmin, displayName, employeeCode, isActive) { Grants = grants };
        user._roleIds.UnionWith(roleIds);
        user._denied.UnionWith(denied);
        return user;
    }

    /// <summary>Pasifleştirir; roller ve özel yetkiler korunur (KK4). Süper admini yalnızca süper admin kapatır (KK5).</summary>
    public Result Deactivate(UserAccess actor, AuditContext audit, DateTimeOffset at) =>
        ChangeActivity(active: false, PermissionChangeKind.UserDeactivated, actor, audit, at);

    public Result Reactivate(UserAccess actor, AuditContext audit, DateTimeOffset at) =>
        ChangeActivity(active: true, PermissionChangeKind.UserReactivated, actor, audit, at);

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
            _changes.Record(PermissionChangeKind.RoleAssigned, role.Id.Value.ToString(), actor.Audit, at);
        }

        return Result.Success();
    }

    public Result UnassignRole(RoleId roleId, AdminActor actor, DateTimeOffset at)
    {
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsSuccess && _roleIds.Remove(roleId))
        {
            _changes.Record(PermissionChangeKind.RoleUnassigned, roleId.Value.ToString(), actor.Audit, at);
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
        _changes.Record(PermissionChangeKind.PermissionGranted, code.ToString(), actor.Audit, at);
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
        _changes.Record(PermissionChangeKind.PermissionRevoked, code.ToString(), actor.Audit, at);
        return Result.Success();
    }

    /// <summary>Yetkiyi kişiden çıkarır; rolünde olsa bile etkisiz olur.</summary>
    public Result Deny(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(code);
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsSuccess && _denied.Add(code))
        {
            _changes.Record(PermissionChangeKind.PermissionDenied, code.ToString(), actor.Audit, at);
        }

        return allowed;
    }

    public Result Undeny(PermissionCode code, AdminActor actor, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(code);
        var allowed = EnsureSameTenant(actor);
        if (allowed.IsSuccess && _denied.Remove(code))
        {
            _changes.Record(PermissionChangeKind.PermissionUndenied, code.ToString(), actor.Audit, at);
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
        if (!IsActive)
        {
            return new HashSet<PermissionCode>();
        }

        var granted = roles
            .Where(r => r.TenantId == TenantId && _roleIds.Contains(r.Id))
            .SelectMany(r => r.Permissions.Codes)
            .Concat(Grants.Codes)
            .Where(c => !_denied.Contains(c))
            .ToImmutableHashSet();

        return granted.Where(c => c.IsView || granted.Contains(c.ViewOfScreen())).ToImmutableHashSet();
    }

    public void ClearPendingChanges() => _changes.Clear();

    private Result ChangeActivity(bool active, PermissionChangeKind kind, UserAccess actor, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (actor.TenantId != TenantId)
        {
            return IdentityErrors.TenantMismatch;
        }

        if (IsSuperAdmin && !actor.IsSuperAdmin)
        {
            return IdentityErrors.SuperAdminProtected;
        }

        if (IsActive != active)
        {
            IsActive = active;
            _changes.Record(kind, $"{EmployeeCode} {DisplayName}".Trim(), audit, at);
        }

        return Result.Success();
    }

    private Result EnsureSameTenant(AdminActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return actor.TenantId == TenantId ? Result.Success() : IdentityErrors.TenantMismatch;
    }
}
