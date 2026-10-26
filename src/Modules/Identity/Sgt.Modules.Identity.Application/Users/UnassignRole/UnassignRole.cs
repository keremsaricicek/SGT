using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.UnassignRole;

public sealed record UnassignRoleCommand(TenantId TenantId, UserId UserId, RoleId RoleId, AuditContext Audit);

public sealed class UnassignRoleHandler(
    AdminActorResolver admins,
    IUserAccessRepository users,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UnassignRoleCommand>
{
    public async Task<Result> Handle(UnassignRoleCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var actor = await admins.Resolve(command.TenantId, command.Audit, ct);
        if (actor.IsFailure)
        {
            return actor;
        }

        var user = await users.Get(command.TenantId, command.UserId, ct);
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var result = user.UnassignRole(command.RoleId, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
