using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Contracts;

/// <summary>
/// Diğer modüllerin sunucu tarafı yetki kontrolü için tek temas noktası (KK10).
/// Yetki kodu biçimi: "&lt;ekran&gt;:&lt;eylem&gt;", ör. "cage.adjustment:approve".
/// </summary>
public interface IPermissionQuery
{
    Task<bool> HasPermission(TenantId tenantId, UserId userId, string permission, CancellationToken ct);
}
