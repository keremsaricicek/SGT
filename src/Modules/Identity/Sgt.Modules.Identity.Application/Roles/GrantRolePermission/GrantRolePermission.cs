using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Roles.GrantRolePermission;

public sealed record GrantRolePermissionCommand(TenantId TenantId, RoleId RoleId, string Permission, AuditContext Audit);

public sealed class GrantRolePermissionHandler(
    AdminActorResolver admins,
    IRoleRepository roles,
    IScreenCatalog screens,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<GrantRolePermissionCommand>
{
    public async Task<Result> Handle(GrantRolePermissionCommand command, CancellationToken ct)
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

        if (!screens.Defines(code.Value))
        {
            return IdentityErrors.UnknownPermission;
        }

        var role = await roles.Get(command.TenantId, command.RoleId, ct);
        if (role is null)
        {
            return IdentityErrors.RoleNotFound;
        }

        var result = role.Grant(code.Value, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
