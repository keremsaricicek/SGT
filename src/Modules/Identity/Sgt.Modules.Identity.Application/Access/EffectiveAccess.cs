using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Access;

/// <summary>
/// Kullanıcının fiilen kullanabildiği yetkiler: etkin yetkiler, yalnızca katalogda tanımlı
/// ve lisansı açık ürünlerin ekranları için (KK4). Core her zaman açıktır.
/// Süper admin, yönetim (Core) ekranlarının tüm yetkilerine sahiptir (KK11).
/// </summary>
public sealed class EffectiveAccess(
    IUserAccessRepository users,
    IRoleRepository roles,
    IScreenCatalog screens,
    ILicensedProducts licenses)
{
    public async Task<IReadOnlySet<PermissionCode>> For(TenantId tenantId, UserId userId, CancellationToken ct)
    {
        var user = await users.Get(tenantId, userId, ct);
        if (user is null)
        {
            return new HashSet<PermissionCode>();
        }

        var assignedRoles = await roles.GetMany(tenantId, user.RoleIds, ct);
        var licensed = await licenses.For(tenantId, ct);
        var productOfScreen = screens.All.ToDictionary(s => s.Code, s => s.Product);

        var superAdminPermissions = user.IsSuperAdmin && user.IsActive
            ? screens.All.Where(s => s.Product == Product.Core).SelectMany(s => s.AllPermissions())
            : [];

        return user.EffectivePermissions(assignedRoles)
            .Concat(superAdminPermissions)
            .Where(code => productOfScreen.TryGetValue(code.Screen, out var product)
                           && (product == Product.Core || licensed.Contains(product)))
            .ToHashSet();
    }
}
