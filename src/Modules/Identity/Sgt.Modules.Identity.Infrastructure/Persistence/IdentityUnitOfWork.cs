using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Değişen aggregate'leri, değişiklik kayıtlarını ve bunların olaylarını (outbox) tek SaveChanges ile yazar.
/// Kayıt okunduktan sonra başkası değiştirdiyse hiçbir şey yazılmaz (<see cref="ConcurrencyConflictException"/>).
/// </summary>
internal sealed class IdentityUnitOfWork(
    IdentityDbContext db,
    TrackedAggregates tracked,
    IntegrationEventSerializer serializer) : IIdentityUnitOfWork
{
    public async Task Commit(CancellationToken ct)
    {
        foreach (var role in tracked.Roles)
        {
            var row = await db.Roles.FindAsync([role.Id.Value], ct);
            if (row is null)
            {
                row = new RoleRow();
                db.Roles.Add(row);
            }

            row.CopyFrom(role);
            Record(role.PendingChanges);
        }

        foreach (var user in tracked.Users)
        {
            var row = await db.Users.FindAsync([user.TenantId.Value, user.UserId.Value], ct);
            if (row is null)
            {
                row = new UserAccessRow();
                db.Users.Add(row);
            }

            row.CopyFrom(user);
            Record(user.PendingChanges);
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConcurrencyConflictException("Rol veya kullanıcı yetkisi başka bir yönetici tarafından değiştirildi.", e);
        }

        foreach (var role in tracked.Roles)
        {
            role.ClearPendingChanges();
        }

        foreach (var user in tracked.Users)
        {
            user.ClearPendingChanges();
        }
    }

    private void Record(IEnumerable<Sgt.Modules.Identity.Domain.PermissionChange> changes)
    {
        foreach (var change in changes)
        {
            db.PermissionChanges.Add(change.ToRow());
            db.Set<OutboxMessage>().Add(serializer.ToOutbox(change.ToIntegrationEvent()));
        }
    }
}
