using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Panel;

namespace Sgt.Modules.Identity.Application.Ports;

public interface IPermissionChangeReader
{
    Task<IReadOnlyList<PermissionChangeView>> Recent(TenantId tenantId, int take, DateTimeOffset? before, CancellationToken ct);
}
