using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;

/// <summary>Varsayılan rolleri oluşturur (KK9). Aynı adda rol varsa ona dokunmaz; tekrar çalıştırmak güvenlidir.</summary>
public sealed record CreateDefaultRolesCommand(TenantId TenantId, AuditContext Audit);

public sealed class CreateDefaultRolesHandler(
    AdminActorResolver admins,
    IRoleRepository roles,
    IScreenCatalog screens,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateDefaultRolesCommand>
{
    public async Task<Result> Handle(CreateDefaultRolesCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var actor = await admins.Resolve(command.TenantId, command.Audit, ct);
        if (actor.IsFailure)
        {
            return actor;
        }

        var existingNames = (await roles.List(command.TenantId, ct)).Select(r => r.Name).ToHashSet();
        foreach (var template in DefaultRoleTemplates.All.Where(t => !existingNames.Contains(t.Name)))
        {
            roles.Add(Build(template, actor.Value));
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }

    private Role Build(RoleTemplate template, AdminActor actor)
    {
        var role = Role.Create(RoleId.New(), template.Name, actor, clock.UtcNow).Value;
        var permissions = screens.All
            .Where(template.Includes)
            .SelectMany(s => s.AllPermissions())
            .OrderByDescending(p => p.IsView); // görme yetkisi önce verilir (KK1)

        foreach (var permission in permissions)
        {
            role.Grant(permission, actor, clock.UtcNow);
        }

        return role;
    }
}
