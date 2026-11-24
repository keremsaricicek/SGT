using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Access;

/// <summary>Diğer modüllere açılan sunucu tarafı yetki kontrolü (KK10).</summary>
public sealed class PermissionChecker(EffectiveAccess access) : IPermissionChecker
{
    public async Task<bool> HasPermission(TenantId tenantId, UserId userId, string permission, CancellationToken ct)
    {
        var code = PermissionCode.Parse(permission);
        if (code.IsFailure)
        {
            return false;
        }

        var granted = await access.For(tenantId, userId, ct);
        return granted.Contains(code.Value);
    }
}
