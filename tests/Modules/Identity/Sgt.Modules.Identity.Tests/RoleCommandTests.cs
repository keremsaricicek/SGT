using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Roles.CreateRole;
using Sgt.Modules.Identity.Application.Roles.GrantRolePermission;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class RoleCommandTests
{
    private readonly InMemoryRoles _roles = new();
    private readonly InMemoryUsers _users = new();
    private readonly CountingUnitOfWork _unitOfWork = new();

    public RoleCommandTests()
    {
        _users.Add(UserAccess.Create(TestData.Audit().Actor, TestData.Tenant, isSuperAdmin: true));
    }

    [Fact]
    public async Task CreateRole_saves_role_and_commits()
    {
        var result = await CreateHandler().Handle(new CreateRoleCommand(TestData.Tenant, "Kasiyer", TestData.Audit()), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, Assert.Single(_roles.Items).Id);
        Assert.Equal(1, _unitOfWork.Commits);
    }

    [Fact]
    public async Task KK5_non_super_admin_cannot_create_role()
    {
        var staff = TestData.Audit() with { Actor = UserId.New() };
        _users.Add(UserAccess.Create(staff.Actor, TestData.Tenant, isSuperAdmin: false));

        var result = await CreateHandler().Handle(new CreateRoleCommand(TestData.Tenant, "Kasiyer", staff), default);

        Assert.Equal(IdentityErrors.NotSuperAdmin, result.Error);
        Assert.Empty(_roles.Items);
        Assert.Equal(0, _unitOfWork.Commits);
    }

    [Fact]
    public async Task KK5_unknown_actor_is_not_super_admin()
    {
        var stranger = TestData.Audit() with { Actor = UserId.New() };

        var result = await CreateHandler().Handle(new CreateRoleCommand(TestData.Tenant, "Kasiyer", stranger), default);

        Assert.Equal(IdentityErrors.NotSuperAdmin, result.Error);
    }

    [Fact]
    public async Task GrantRolePermission_grants_and_commits()
    {
        var role = TestData.NewRole();
        _roles.Add(role);

        var result = await GrantHandler().Handle(
            new GrantRolePermissionCommand(TestData.Tenant, role.Id, "cage.adjustment:view", TestData.Audit()), default);

        Assert.True(result.IsSuccess);
        Assert.True(role.Permissions.Contains(TestData.CageView));
        Assert.Equal(1, _unitOfWork.Commits);
    }

    [Fact]
    public async Task GrantRolePermission_rejects_invalid_code_and_unknown_role()
    {
        var invalid = await GrantHandler().Handle(
            new GrantRolePermissionCommand(TestData.Tenant, RoleId.New(), "bozuk", TestData.Audit()), default);
        var unknown = await GrantHandler().Handle(
            new GrantRolePermissionCommand(TestData.Tenant, RoleId.New(), "cage.adjustment:view", TestData.Audit()), default);

        Assert.Equal(IdentityErrors.InvalidPermissionCode, invalid.Error);
        Assert.Equal(IdentityErrors.RoleNotFound, unknown.Error);
        Assert.Equal(0, _unitOfWork.Commits);
    }

    [Fact]
    public async Task GrantRolePermission_does_not_commit_when_domain_rule_fails()
    {
        var role = TestData.NewRole();
        _roles.Add(role);

        var result = await GrantHandler().Handle(
            new GrantRolePermissionCommand(TestData.Tenant, role.Id, "cage.adjustment:operate", TestData.Audit()), default);

        Assert.Equal(IdentityErrors.ViewRequired, result.Error);
        Assert.Equal(0, _unitOfWork.Commits);
    }

    private CreateRoleHandler CreateHandler() =>
        new(new AdminActorResolver(_users), _roles, _unitOfWork, new FixedClock());

    private GrantRolePermissionHandler GrantHandler() =>
        new(new AdminActorResolver(_users), _roles, _unitOfWork, new FixedClock());
}
