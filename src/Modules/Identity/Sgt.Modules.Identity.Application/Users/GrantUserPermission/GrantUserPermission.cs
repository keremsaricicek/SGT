using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.GrantUserPermission;

/// <summary>Kişiye rolünden bağımsız yetki ekler.</summary>
public sealed record GrantUserPermissionCommand(TenantId TenantId, UserId UserId, string Permission, AuditContext Audit);

public sealed class GrantUserPermissionHandler(
    AdminActorResolver admins,
    IUserAccessRepository users,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<GrantUserPermissionCommand>
{
    public async Task<Result> Handle(GrantUserPermissionCommand command, CancellationToken ct)
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

        var user = await users.Get(command.TenantId, command.UserId, ct);
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var result = user.Grant(code.Value, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
