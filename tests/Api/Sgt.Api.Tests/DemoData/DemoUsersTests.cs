using System.Net.Http.Json;
using Sgt.DemoData;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Panel;

namespace Sgt.Api.Tests.DemoData;

/// <summary>Spec 012 US2 (KK6): her rol için demo kullanıcısı; menüsü rolüne uygun.</summary>
[Collection(DemoSeedGroup.Name)]
public sealed class DemoUsersTests(DemoSeedFixture fx)
{
    [Fact]
    public async Task KK6_every_demo_user_is_registered_with_its_role()
    {
        var users = await fx.Query<ListUsersQuery, IReadOnlyList<UserSummary>>(new ListUsersQuery(ApiFactory.Tenant));
        var roles = await fx.Query<ListRolesQuery, IReadOnlyList<RoleView>>(new ListRolesQuery(ApiFactory.Tenant));

        Assert.Equal(6, DemoUsers.All.Count);
        foreach (var demo in DemoUsers.Staff)
        {
            var user = Assert.Single(users, u => u.UserId == demo.Id.Value);
            Assert.Equal(demo.DisplayName, user.DisplayName);
            var access = await fx.Query<GetUserAccessQuery, UserAccessView?>(new GetUserAccessQuery(ApiFactory.Tenant, demo.Id));
            var role = Assert.Single(roles, r => r.Name == demo.Role);
            Assert.Equal([role.Id], access!.RoleIds);
        }
    }

    [Fact]
    public async Task KK6_cashier_sees_cage_but_not_live_game_or_admin()
    {
        var menu = await Menu(DemoUsers.Cashier);

        Assert.Contains("open", menu["cage.shift"]);
        Assert.Contains("sell", menu["cage.chips"]);
        Assert.Contains("exchange", menu["cage.exchange"]);
        Assert.DoesNotContain("live.rating", menu.Keys);
        Assert.DoesNotContain("admin.users", menu.Keys);
    }

    [Fact]
    public async Task KK6_host_requests_but_cannot_approve_comps()
    {
        var menu = await Menu(DemoUsers.Host);

        Assert.Contains("request", menu["loyalty.comp"]);
        Assert.DoesNotContain("approve", menu["loyalty.comp"]);
        Assert.Contains("player.search", menu.Keys);
        Assert.DoesNotContain("cage.shift", menu.Keys);
    }

    [Fact]
    public async Task KK6_floor_roles_see_their_own_area()
    {
        var pit = await Menu(DemoUsers.PitBoss);
        var slot = await Menu(DemoUsers.SlotManager);
        var manager = await Menu(DemoUsers.CasinoManager);

        Assert.Contains("start", pit["live.rating"]);
        Assert.DoesNotContain("slot.floor", pit.Keys);
        Assert.Contains("enter", slot["slot.manual_entry"]);
        Assert.DoesNotContain("live.rating", slot.Keys);
        Assert.Contains("approve", manager["loyalty.comp"]);
        Assert.Contains("report.daily", manager.Keys);
    }

    [Fact]
    public async Task KK6_super_admin_manages_the_permission_panel()
    {
        var menu = await Menu(DemoUsers.SuperAdmin);

        Assert.Contains("admin.users", menu.Keys);
        Assert.Contains("admin.roles", menu.Keys);
    }

    private async Task<Dictionary<string, IReadOnlyList<string>>> Menu(DemoUser user)
    {
        var items = await fx.Api.ClientFor(user.Id).GetFromJsonAsync<List<MenuItem>>("/api/v1/identity/me/menu");
        return items!.ToDictionary(i => i.Screen, i => i.Actions);
    }
}
