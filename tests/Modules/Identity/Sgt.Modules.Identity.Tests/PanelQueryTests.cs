using Sgt.Modules.Identity.Application.Panel;

namespace Sgt.Modules.Identity.Tests;

public sealed class PanelQueryTests
{
    private readonly InMemoryRoles _roles = new();
    private readonly InMemoryUsers _users = new();

    [Fact]
    public async Task ListRoles_returns_only_tenant_roles_sorted_by_name()
    {
        var cashier = TestData.NewRole("Kasiyer");
        cashier.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);
        _roles.Add(cashier);
        _roles.Add(TestData.NewRole("Bar"));
        _roles.Add(TestData.NewRole("Yabancı", TestData.OtherTenant));

        var roles = await new ListRolesHandler(_roles).Handle(new ListRolesQuery(TestData.Tenant), default);

        Assert.Equal(["Bar", "Kasiyer"], roles.Select(r => r.Name));
        Assert.Equal(["cage.adjustment:view"], roles[1].Permissions);
    }

    [Fact]
    public async Task GetUserAccess_shows_roles_grants_denies_and_effective_permissions()
    {
        var role = TestData.NewRole();
        role.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);
        role.Grant(TestData.CageOperate, TestData.SuperAdmin(), TestData.Now);
        _roles.Add(role);
        var user = TestData.NewUser();
        user.AssignRole(role, TestData.SuperAdmin(), TestData.Now);
        user.Deny(TestData.CageOperate, TestData.SuperAdmin(), TestData.Now);
        _users.Add(user);

        var view = await new GetUserAccessHandler(_users, _roles).Handle(new GetUserAccessQuery(TestData.Tenant, user.UserId), default);

        Assert.NotNull(view);
        Assert.Equal([role.Id.Value], view.RoleIds);
        Assert.Equal(["cage.adjustment:operate"], view.Denied);
        Assert.Equal(["cage.adjustment:view"], view.Effective);
    }

    [Fact]
    public async Task GetUserAccess_returns_null_for_unknown_user()
    {
        var view = await new GetUserAccessHandler(_users, _roles)
            .Handle(new GetUserAccessQuery(TestData.Tenant, Sgt.BuildingBlocks.UserId.New()), default);

        Assert.Null(view);
    }
}
