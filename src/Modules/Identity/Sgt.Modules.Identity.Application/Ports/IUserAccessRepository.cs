using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Ports;

public interface IUserAccessRepository
{
    Task<UserAccess?> Get(TenantId tenantId, UserId userId, CancellationToken ct);

    void Add(UserAccess userAccess);
}
