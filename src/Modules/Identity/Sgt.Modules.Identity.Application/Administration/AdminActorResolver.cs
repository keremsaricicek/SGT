using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Administration;

/// <summary>İşlemi yapan kişinin süper admin olup olmadığını kontrol eder (KK5).</summary>
public sealed class AdminActorResolver(IUserAccessRepository users)
{
    public async Task<Result<AdminActor>> Resolve(TenantId tenantId, AuditContext audit, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var actor = await users.Get(tenantId, audit.Actor, ct);
        return AdminActor.From(tenantId, audit, isSuperAdmin: actor?.IsSuperAdmin ?? false);
    }
}
