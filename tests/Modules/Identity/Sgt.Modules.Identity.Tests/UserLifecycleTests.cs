using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Panel;
using Sgt.Modules.Identity.Application.Users.DeactivateUser;
using Sgt.Modules.Identity.Application.Users.ReactivateUser;
using Sgt.Modules.Identity.Application.Users.RegisterUser;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class UserLifecycleTests
{
    private readonly InMemoryRoles _roles = new();
    private readonly InMemoryUsers _users = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly UserAccess _admin = UserAccess.Create(TestData.Audit().Actor, TestData.Tenant, isSuperAdmin: true);

    public UserLifecycleTests()
    {
        _users.Add(_admin);
    }

    [Fact]
    public async Task KK1_KK6_register_creates_active_user_and_records_change()
    {
        var newUser = UserId.New();

        var result = await Register(newUser, "Ayşe Yılmaz", "P-001");

        Assert.True(result.IsSuccess);
        var user = _users.Items.Single(u => u.UserId == newUser);
        Assert.True(user.IsActive);
        Assert.Equal("Ayşe Yılmaz", user.DisplayName);
        Assert.Equal("P-001", user.EmployeeCode);
        Assert.Equal(PermissionChangeKind.UserCreated, Assert.Single(user.PendingChanges).Kind);
    }

    [Theory]
    [InlineData("", "P-1")]
    [InlineData("Ali", " ")]
    public async Task KK1_name_and_employee_code_are_required(string name, string code)
    {
        var result = await Register(UserId.New(), name, code);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, new[] { IdentityErrors.DisplayNameRequired, IdentityErrors.EmployeeCodeRequired });
    }

    [Fact]
    public async Task KK1_KK2_employee_code_and_identity_are_unique()
    {
        var existing = UserId.New();
        await Register(existing, "Ali", "P-7");

        Assert.Equal(IdentityErrors.EmployeeCodeTaken, (await Register(UserId.New(), "Veli", "p-7")).Error);
        Assert.Equal(IdentityErrors.UserAlreadyExists, (await Register(existing, "Ali", "P-8")).Error);
    }

    [Fact]
    public async Task KK3_KK4_deactivated_user_has_no_permissions_until_reactivated()
    {
        var role = TestData.NewRole();
        role.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);
        _roles.Add(role);
        var staff = UserId.New();
        await Register(staff, "Ali", "P-2");
        var user = _users.Items.Single(u => u.UserId == staff);
        user.AssignRole(role, TestData.SuperAdmin(), TestData.Now);

        Assert.True((await new DeactivateUserHandler(_users, _unitOfWork, new FixedClock())
            .Handle(new DeactivateUserCommand(TestData.Tenant, staff, TestData.Audit()), default)).IsSuccess);
        Assert.Empty(user.EffectivePermissions([role]));

        await new ReactivateUserHandler(_users, _unitOfWork, new FixedClock())
            .Handle(new ReactivateUserCommand(TestData.Tenant, staff, TestData.Audit()), default);
        Assert.Equal([TestData.CageView], user.EffectivePermissions([role]));
    }

    [Fact]
    public async Task KK5_staff_cannot_deactivate_super_admin_and_last_super_admin_is_protected()
    {
        var staff = UserId.New();
        await Register(staff, "Ali", "P-3");
        var staffAudit = TestData.Audit() with { Actor = staff };

        var byStaff = await new DeactivateUserHandler(_users, _unitOfWork, new FixedClock())
            .Handle(new DeactivateUserCommand(TestData.Tenant, _admin.UserId, staffAudit), default);
        var lastOne = await new DeactivateUserHandler(_users, _unitOfWork, new FixedClock())
            .Handle(new DeactivateUserCommand(TestData.Tenant, _admin.UserId, TestData.Audit()), default);

        Assert.Equal(IdentityErrors.SuperAdminProtected, byStaff.Error);
        Assert.Equal(IdentityErrors.LastSuperAdmin, lastOne.Error);
        Assert.True(_admin.IsActive);
    }

    [Fact]
    public async Task KK7_list_users_shows_profile_and_status()
    {
        await Register(UserId.New(), "Zeynep", "P-9");

        var users = await new ListUsersHandler(_users).Handle(new ListUsersQuery(TestData.Tenant), default);

        Assert.Contains(users, u => u.DisplayName == "Zeynep" && u.EmployeeCode == "P-9" && u.IsActive && !u.IsSuperAdmin);
    }

    private Task<Result> Register(UserId id, string name, string code) =>
        new RegisterUserHandler(_users, _unitOfWork, new FixedClock())
            .Handle(new RegisterUserCommand(TestData.Tenant, id, name, code, TestData.Audit()), default);
}
