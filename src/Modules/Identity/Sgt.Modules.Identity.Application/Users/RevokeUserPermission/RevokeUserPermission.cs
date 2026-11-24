using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Users.RevokeUserPermission;

/// <summary>Kişiye özel eklenmiş yetkiyi geri alır.</summary>
public sealed record RevokeUserPermissionCommand(TenantId TenantId, UserId UserId, string Permission, AuditContext Audit);

public sealed class RevokeUserPermissionHandler(
    AdminActorResolver admins,
    IUserAccessRepository users,
    IIdentityUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RevokeUserPermissionCommand>
{
    public async Task<Result> Handle(RevokeUserPermissionCommand command, CancellationToken ct)
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

        var result = user.Revoke(code.Value, actor.Value, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
