using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Roles.RevokeRolePermission;

public sealed record RevokeRolePermissionCommand(TenantId TenantId, RoleId RoleId, string Permission, AuditContext Audit);

public sealed class RevokeRolePermissionHandler(
    AdminActorResolver admins,
    IRoleRepository roles,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RevokeRolePermissionCommand>
{
    public async Task<Result> Handle(RevokeRolePermissionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var actor = await admins.Resolve(command.TenantId, command.Audit, ct);
        if (actor.IsFailure)
        {
            return actor;
        }

        var code = PermissionCode.Parse(command.Permission);
        if (code.IsFailure)
        {
            return code;
        }

        var role = await roles.Get(command.TenantId, command.RoleId, ct);
        if (role is null)
        {
            return IdentityErrors.RoleNotFound;
        }

        var result = role.Revoke(code.Value, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
