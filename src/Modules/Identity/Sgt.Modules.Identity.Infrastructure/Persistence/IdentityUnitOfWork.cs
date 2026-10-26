using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Değişen aggregate'leri ve onların değişiklik kayıtlarını tek SaveChanges (tek transaction) ile yazar.
/// Kayıt okunduktan sonra başkası değiştirdiyse hiçbir şey yazılmaz (<see cref="ConcurrencyConflictException"/>).
/// </summary>
internal sealed class IdentityUnitOfWork(IdentityDbContext db, TrackedAggregates tracked) : IUnitOfWork
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
            db.PermissionChanges.AddRange(role.PendingChanges.Select(c => c.ToRow()));
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
            db.PermissionChanges.AddRange(user.PendingChanges.Select(c => c.ToRow()));
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
}
