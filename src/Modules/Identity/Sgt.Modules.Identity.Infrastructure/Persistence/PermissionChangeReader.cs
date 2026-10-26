using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Panel;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

internal sealed class PermissionChangeReader(IdentityDbContext db) : IPermissionChangeReader
{
    public async Task<IReadOnlyList<PermissionChangeView>> Recent(
        TenantId tenantId, int take, DateTimeOffset? before, CancellationToken ct) =>
        await db.PermissionChanges.AsNoTracking()
            .Where(c => c.TenantId == tenantId.Value && (before == null || c.OccurredAt < before))
            .OrderByDescending(c => c.OccurredAt)
            .Take(take)
            .Select(c => new PermissionChangeView(
                c.Id, c.Target, c.TargetId, c.Kind, c.Item, c.ActorId, c.Terminal, c.ReasonCode, c.CorrelationId, c.OccurredAt))
            .ToListAsync(ct);
}
