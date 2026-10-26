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

    public void Add(UserAccess userAccess) => tracked.Users.Add(userAccess);
}
