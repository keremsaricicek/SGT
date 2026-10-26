using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Roles.CreateRole;

public sealed record CreateRoleCommand(TenantId TenantId, string Name, AuditContext Audit);

public sealed class CreateRoleHandler(
    AdminActorResolver admins,
    IRoleRepository roles,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateRoleCommand, RoleId>
{
    public async Task<Result<RoleId>> Handle(CreateRoleCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var actor = await admins.Resolve(command.TenantId, command.Audit, ct);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        var role = Role.Create(RoleId.New(), command.Name, actor.Value, clock.UtcNow);
        if (role.IsFailure)
        {
            return role.Error;
        }

        roles.Add(role.Value);
        await unitOfWork.Commit(ct);
        return role.Value.Id;
    }
}
