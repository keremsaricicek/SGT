using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Access;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class AccessQueryTests
{
    private static readonly FixedScreens Screens = new(
        new ScreenDefinition("admin.roles", Product.Core),
        new ScreenDefinition("cage.adjustment", Product.SPlus),
        new ScreenDefinition("hotel.checkin", Product.SVista));

    private readonly InMemoryRoles _roles = new();
    private readonly InMemoryUsers _users = new();
    private readonly UserAccess _user = TestData.NewUser();

    public AccessQueryTests()
    {
        var role = TestData.NewRole();
        foreach (var code in new[] { "admin.roles:view", "cage.adjustment:view", "cage.adjustment:operate", "hotel.checkin:view", "unknown.screen:view" })
        {
            role.Grant(TestData.Code(code), TestData.SuperAdmin(), TestData.Now);
        }

        _roles.Add(role);
        _user.AssignRole(role, TestData.SuperAdmin(), TestData.Now);
        _users.Add(_user);
    }

    [Fact]
    public async Task KK4_menu_shows_only_licensed_catalog_screens_in_catalog_order()
    {
        var menu = await new GetMyMenuHandler(Access(Product.SPlus), Screens)
            .Handle(new GetMyMenuQuery(TestData.Tenant, _user.UserId), default);

        Assert.Equal(["admin.roles", "cage.adjustment"], menu.Select(m => m.Screen));
        Assert.Equal(["operate", "view"], menu[1].Actions);
    }

    [Fact]
    public async Task KK10_permission_query_respects_license()
    {
        var withoutVista = new PermissionQuery(Access(Product.SPlus));
        var withVista = new PermissionQuery(Access(Product.SPlus, Product.SVista));

        Assert.True(await withoutVista.HasPermission(TestData.Tenant, _user.UserId, "cage.adjustment:operate", default));
        Assert.False(await withoutVista.HasPermission(TestData.Tenant, _user.UserId, "hotel.checkin:view", default));
        Assert.True(await withVista.HasPermission(TestData.Tenant, _user.UserId, "hotel.checkin:view", default));
    }

    [Fact]
    public async Task KK10_unknown_user_or_malformed_code_has_no_permission()
    {
        var query = new PermissionQuery(Access(Product.SPlus));

        Assert.False(await query.HasPermission(TestData.Tenant, UserId.New(), "admin.roles:view", default));
        Assert.False(await query.HasPermission(TestData.Tenant, _user.UserId, "bozuk", default));
        Assert.False(await query.HasPermission(TestData.OtherTenant, _user.UserId, "admin.roles:view", default));
    }

    private EffectiveAccess Access(params Product[] licensed) =>
        new(_users, _roles, Screens, new FixedLicenses(licensed));
}
