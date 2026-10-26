using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.ReactivateUser;

/// <summary>Pasif kullanıcıyı yeniden aktifleştirir; rolleri ve özel yetkileri geri gelir.</summary>
public sealed record ReactivateUserCommand(TenantId TenantId, UserId UserId, AuditContext Audit);

public sealed class ReactivateUserHandler(
    IUserAccessRepository users,
    IIdentityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReactivateUserCommand>
{
    public async Task<Result> Handle(ReactivateUserCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var actor = await users.Get(command.TenantId, command.Audit.Actor, ct);
        var target = await users.Get(command.TenantId, command.UserId, ct);
        if (actor is null || target is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var result = target.Reactivate(actor, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
