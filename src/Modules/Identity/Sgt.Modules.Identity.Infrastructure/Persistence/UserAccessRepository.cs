using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

internal sealed class UserAccessRepository(IdentityDbContext db, TrackedAggregates tracked) : IUserAccessRepository
{
    public async Task<UserAccess?> Get(TenantId tenantId, UserId userId, CancellationToken ct)
    {
        var row = await db.Users.SingleOrDefaultAsync(u => u.TenantId == tenantId.Value && u.UserId == userId.Value, ct);
        if (row is null)
        {
            return null;
        }

        var user = row.ToDomain();
        tracked.Users.Add(user);
        return user;
    }

    public async Task<IReadOnlyList<UserAccess>> List(TenantId tenantId, CancellationToken ct)
    {
        var rows = await db.Users.AsNoTracking().Where(u => u.TenantId == tenantId.Value).ToListAsync(ct);
        return rows.Select(r => r.ToDomain()).ToList();
    }

    public Task<bool> EmployeeCodeExists(TenantId tenantId, string employeeCode, CancellationToken ct)
    {
        var normalized = employeeCode.Trim().ToUpperInvariant();
        return db.Users.AnyAsync(u => u.TenantId == tenantId.Value && u.EmployeeCode == normalized, ct);
    }

    public Task<int> CountActiveSuperAdmins(TenantId tenantId, CancellationToken ct) =>
        db.Users.CountAsync(u => u.TenantId == tenantId.Value && u.IsSuperAdmin && u.IsActive, ct);

    public void Add(UserAccess userAccess) => tracked.Users.Add(userAccess);
}
