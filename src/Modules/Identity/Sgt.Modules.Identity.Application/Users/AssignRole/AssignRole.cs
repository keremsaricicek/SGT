using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.AssignRole;

public sealed record AssignRoleCommand(TenantId TenantId, UserId UserId, RoleId RoleId, AuditContext Audit);

public sealed class AssignRoleHandler(
    AdminActorResolver admins,
    IUserAccessRepository users,
    IRoleRepository roles,
    IIdentityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AssignRoleCommand>
{
    public async Task<Result> Handle(AssignRoleCommand command, CancellationToken ct)
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

        var role = await roles.Get(command.TenantId, command.RoleId, ct);
        if (role is null)
        {
            return IdentityErrors.RoleNotFound;
        }

        var result = user.AssignRole(role, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
