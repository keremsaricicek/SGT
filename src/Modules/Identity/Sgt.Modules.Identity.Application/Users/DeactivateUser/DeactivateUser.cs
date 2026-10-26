using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.DeactivateUser;

/// <summary>Kullanıcıyı pasifleştirir; tüm yetkileri etkisiz olur (Spec 003 KK3–KK5).</summary>
public sealed record DeactivateUserCommand(TenantId TenantId, UserId UserId, AuditContext Audit);

public sealed class DeactivateUserHandler(
    IUserAccessRepository users,
    IIdentityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<DeactivateUserCommand>
{
    public async Task<Result> Handle(DeactivateUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var actor = await users.Get(command.TenantId, command.Audit.Actor, ct);
        var target = await users.Get(command.TenantId, command.UserId, ct);
        if (actor is null || target is null)
        {
            return IdentityErrors.UserNotFound;
        }

        // Son aktif süper admin kapatılırsa kimse yetki veremez (KK5).
        if (target.IsSuperAdmin && target.IsActive && actor.IsSuperAdmin
            && await users.CountActiveSuperAdmins(command.TenantId, ct) <= 1)
        {
            return IdentityErrors.LastSuperAdmin;
        }

        var result = target.Deactivate(actor, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
