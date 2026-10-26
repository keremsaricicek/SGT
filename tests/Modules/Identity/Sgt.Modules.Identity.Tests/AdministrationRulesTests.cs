using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class AdministrationRulesTests
{
    [Fact]
    public void KK5_only_super_admin_can_administer()
    {
        var result = AdminActor.From(TestData.Tenant, TestData.Audit(), isSuperAdmin: false);

        Assert.Equal(IdentityErrors.NotSuperAdmin, result.Error);
    }

    [Fact]
    public void KK6_every_change_is_recorded_with_actor_reason_and_time()
    {
        var role = TestData.NewRole();
        role.ClearPendingChanges();

        role.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);

        var change = Assert.Single(role.PendingChanges);
        Assert.Equal(PermissionChangeKind.PermissionGranted, change.Kind);
        Assert.Equal(PermissionChangeTarget.Role, change.Target);
        Assert.Equal(role.Id.Value, change.TargetId);
        Assert.Equal("cage.adjustment:view", change.Item);
        Assert.Equal(TestData.Audit(), change.Audit);
        Assert.Equal(TestData.Now, change.OccurredAt);
    }

    [Fact]
    public void KK7_repeating_a_grant_or_revoking_missing_permission_records_nothing()
    {
        var role = TestData.NewRole();
        role.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);
        role.ClearPendingChanges();

        Assert.True(role.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now).IsSuccess);
        Assert.True(role.Revoke(TestData.SlotView, TestData.SuperAdmin(), TestData.Now).IsSuccess);
        Assert.Empty(role.PendingChanges);
    }

    [Fact]
    public void KK8_role_of_another_tenant_cannot_be_assigned()
    {
        var foreignRole = TestData.NewRole(tenant: TestData.OtherTenant);
        var user = TestData.NewUser();

        var result = user.AssignRole(foreignRole, TestData.SuperAdmin(), TestData.Now);

        Assert.Equal(IdentityErrors.TenantMismatch, result.Error);
    }

    [Fact]
    public void KK8_admin_of_another_tenant_cannot_change_role()
    {
        var role = TestData.NewRole();

        var result = role.Grant(TestData.CageView, TestData.SuperAdmin(TestData.OtherTenant), TestData.Now);

        Assert.Equal(IdentityErrors.TenantMismatch, result.Error);
    }

    [Fact]
    public void Role_name_is_required()
    {
        var result = Role.Create(RoleId.New(), " ", TestData.SuperAdmin(), TestData.Now);

        Assert.Equal(IdentityErrors.RoleNameRequired, result.Error);
    }
}
