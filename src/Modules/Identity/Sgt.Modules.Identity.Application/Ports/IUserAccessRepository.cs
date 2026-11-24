using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Ports;

public interface IUserAccessRepository
{
    Task<UserAccess?> Get(TenantId tenantId, UserId userId, CancellationToken ct);

    Task<IReadOnlyList<UserAccess>> List(TenantId tenantId, CancellationToken ct);

    Task<bool> EmployeeCodeExists(TenantId tenantId, string employeeCode, CancellationToken ct);

    Task<int> CountActiveSuperAdmins(TenantId tenantId, CancellationToken ct);

    void Add(UserAccess userAccess);
}
