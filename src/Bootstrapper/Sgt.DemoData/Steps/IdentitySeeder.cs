using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Panel;
using Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;
using Sgt.Modules.Identity.Application.Roles.CreateRole;
using Sgt.Modules.Identity.Application.Roles.GrantRolePermission;
using Sgt.Modules.Identity.Application.Users.AssignRole;
using Sgt.Modules.Identity.Application.Users.RegisterUser;
using Sgt.Modules.Identity.Domain;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Demo kullanıcıları ve rolleri (Spec 012 KK6). Varsayılan roller (casino müdürü, pit boss, slot müdürü) hazır şablondan,
/// "Kasiyer" ve "Host" rolleri ekran yetkileriyle oluşturulur. Tüm yönetim komutlarının aktörü süper admindir.
/// </summary>
public sealed class IdentitySeeder(DemoCommands commands) : IDemoStep
{
    /// <summary>Görme yetkisi önce verilir (Spec 003 KK1); liste bu sırayı korur.</summary>
    private static readonly Dictionary<string, string[]> CustomRoles = new(StringComparer.Ordinal)
    {
        [DemoUsers.CashierRole] =
        [
            "cage.shift:view", "cage.shift:open", "cage.shift:close", "cage.shift:count",
            "cage.cash:view", "cage.cash:cash_in", "cage.cash:cash_out",
            "cage.chips:view", "cage.chips:sell", "cage.chips:redeem",
            "cage.exchange:view", "cage.exchange:exchange",
            "cage.adjustment:view", "cage.adjustment:request",
            "cage.summary:view", "player.search:view",
        ],
        [DemoUsers.HostRole] =
        [
            "player.search:view", "player.locator:view", "player.history:view",
            "player.profile:view", "player.profile:create", "player.profile:edit", "player.profile:issue_card",
            "loyalty.points:view", "loyalty.comp:view", "loyalty.comp:request", "loyalty.host_portfolio:view",
        ],
    };

    private static UserId Admin => DemoUsers.SuperAdmin.Id;

    public int Order => 10;

    public string Name => "Kullanıcılar ve roller";

    public async Task Run(DemoRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        await commands.Do<ICommandHandler<CreateDefaultRolesCommand>>(
            "varsayılan roller", h => h.Handle(new CreateDefaultRolesCommand(run.Tenant, run.Audit(Admin)), ct));

        var roles = await RoleIds(run, ct);
        foreach (var (name, permissions) in CustomRoles.Where(r => !roles.ContainsKey(r.Key)))
        {
            var roleId = await commands.Ask<ICommandHandler<CreateRoleCommand, RoleId>, RoleId>(
                "rol", h => h.Handle(new CreateRoleCommand(run.Tenant, name, run.Audit(Admin)), ct));
            foreach (var permission in permissions)
            {
                await commands.Do<ICommandHandler<GrantRolePermissionCommand>>(
                    "rol yetkisi", h => h.Handle(new GrantRolePermissionCommand(run.Tenant, roleId, permission, run.Audit(Admin)), ct));
            }

            roles[name] = roleId;
        }

        foreach (var user in DemoUsers.Staff)
        {
            var registered = await commands.Try<ICommandHandler<RegisterUserCommand>>(
                h => h.Handle(new RegisterUserCommand(run.Tenant, user.Id, user.DisplayName, user.EmployeeCode, run.Audit(Admin)), ct));
            if (registered.IsFailure && registered.Error != IdentityErrors.UserAlreadyExists)
            {
                throw new DemoSeedException($"Demo kullanıcısı kaydedilemedi ({user.UserName}): {registered.Error.Code} {registered.Error.Message}");
            }

            await commands.Do<ICommandHandler<AssignRoleCommand>>(
                "rol ataması", h => h.Handle(new AssignRoleCommand(run.Tenant, user.Id, roles[user.Role!], run.Audit(Admin)), ct));
        }
    }

    private async Task<Dictionary<string, RoleId>> RoleIds(DemoRun run, CancellationToken ct) =>
        (await commands.Query<ListRolesQuery, IReadOnlyList<RoleView>>(new ListRolesQuery(run.Tenant), ct))
            .ToDictionary(r => r.Name, r => new RoleId(r.Id), StringComparer.Ordinal);
}
