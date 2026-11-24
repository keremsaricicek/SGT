using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

internal sealed class RoleRepository(IdentityDbContext db, TrackedAggregates tracked) : IRoleRepository
{
    public async Task<Role?> Get(TenantId tenantId, RoleId roleId, CancellationToken ct)
    {
        var row = await db.Roles.SingleOrDefaultAsync(r => r.TenantId == tenantId.Value && r.Id == roleId.Value, ct);
        return row is null ? null : Track(row.ToDomain());
    }

    public async Task<IReadOnlyList<Role>> GetMany(TenantId tenantId, IEnumerable<RoleId> roleIds, CancellationToken ct)
    {
        var ids = roleIds.Select(r => r.Value).ToList();
        var rows = await db.Roles.Where(r => r.TenantId == tenantId.Value && ids.Contains(r.Id)).ToListAsync(ct);
        return rows.Select(r => Track(r.ToDomain())).ToList();
    }

    public async Task<IReadOnlyList<Role>> List(TenantId tenantId, CancellationToken ct)
    {
        var rows = await db.Roles.Where(r => r.TenantId == tenantId.Value).ToListAsync(ct);
        return rows.Select(r => Track(r.ToDomain())).ToList();
    }

    public void Add(Role role) => Track(role);

    private Role Track(Role role)
    {
        tracked.Roles.Add(role);
        return role;
    }
}
