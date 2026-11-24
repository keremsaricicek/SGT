using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.DenyUserPermission;

/// <summary>Yetkiyi kişiden çıkarır; rolünde olsa bile etkisiz olur.</summary>
public sealed record DenyUserPermissionCommand(TenantId TenantId, UserId UserId, string Permission, AuditContext Audit);

public sealed class DenyUserPermissionHandler(
    AdminActorResolver admins,
    IUserAccessRepository users,
    IScreenCatalog screens,
    IIdentityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<DenyUserPermissionCommand>
{
    public async Task<Result> Handle(DenyUserPermissionCommand command, CancellationToken ct)
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

        var user = await users.Get(command.TenantId, command.UserId, ct);
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var result = user.Deny(code.Value, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
