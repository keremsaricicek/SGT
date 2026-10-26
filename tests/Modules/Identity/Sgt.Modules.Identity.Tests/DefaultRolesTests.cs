using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class DefaultRolesTests
{
    private readonly InMemoryRoles _roles = new();
    private readonly InMemoryUsers _users = new();
    private readonly CountingUnitOfWork _unitOfWork = new();

    public DefaultRolesTests()
    {
        _users.Add(UserAccess.Create(TestData.Audit().Actor, TestData.Tenant, isSuperAdmin: true));
    }

    [Fact]
    public async Task KK9_creates_the_five_default_roles_with_their_areas()
    {
        var result = await Handler().Handle(new CreateDefaultRolesCommand(TestData.Tenant, TestData.Audit()), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, _roles.Items.Count);

        var slotManager = Role(DefaultRoleTemplates.SlotManager);
        Assert.True(slotManager.Permissions.Contains(TestData.Code("slot.jackpot:approve")));
        Assert.True(slotManager.Permissions.Contains(TestData.Code("player.locator:view")));
        Assert.False(slotManager.Permissions.Contains(TestData.Code("live.rating:view")));

        var pitBoss = Role(DefaultRoleTemplates.PitBoss);
        Assert.True(pitBoss.Permissions.Contains(TestData.Code("live.rating:start")));
        Assert.False(pitBoss.Permissions.Contains(TestData.Code("slot.floor:view")));

        var casinoManager = Role(DefaultRoleTemplates.CasinoManager);
        Assert.True(casinoManager.Permissions.Contains(TestData.Code("hotel.checkin:operate")));
        Assert.False(casinoManager.Permissions.Contains(TestData.Code("admin.roles:view")));

        var hotelStaff = Role(DefaultRoleTemplates.HotelStaff);
        Assert.Equal([TestData.Code("hotel.checkin:operate"), TestData.Code("hotel.checkin:view")],
            hotelStaff.Permissions.Codes.OrderBy(c => c.ToString()));
    }

    [Fact]
    public async Task KK9_running_twice_does_not_duplicate_roles()
    {
        await Handler().Handle(new CreateDefaultRolesCommand(TestData.Tenant, TestData.Audit()), default);
        await Handler().Handle(new CreateDefaultRolesCommand(TestData.Tenant, TestData.Audit()), default);

        Assert.Equal(5, _roles.Items.Count);
    }

    [Fact]
    public async Task KK5_only_super_admin_can_create_default_roles()
    {
        var staff = TestData.Audit() with { Actor = Sgt.BuildingBlocks.UserId.New() };

        var result = await Handler().Handle(new CreateDefaultRolesCommand(TestData.Tenant, staff), default);

        Assert.Equal(IdentityErrors.NotSuperAdmin, result.Error);
        Assert.Empty(_roles.Items);
    }

    private Role Role(string name) => _roles.Items.Single(r => r.Name == name);

    private CreateDefaultRolesHandler Handler() =>
        new(new AdminActorResolver(_users), _roles, TestData.Screens, _unitOfWork, new FixedClock());
}
