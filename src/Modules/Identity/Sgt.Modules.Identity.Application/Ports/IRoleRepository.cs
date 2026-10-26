using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Ports;

public interface IRoleRepository
{
    Task<Role?> Get(TenantId tenantId, RoleId roleId, CancellationToken ct);

    Task<IReadOnlyList<Role>> GetMany(TenantId tenantId, IEnumerable<RoleId> roleIds, CancellationToken ct);

    void Add(Role role);
}
