using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class EffectivePermissionsTests
{
    [Fact]
    public void KK3_union_of_roles_and_user_grants_minus_denies()
    {
        var cashier = TestData.NewRole("Kasiyer");
        cashier.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);
        cashier.Grant(TestData.CageOperate, TestData.SuperAdmin(), TestData.Now);

        var user = TestData.NewUser();
        user.AssignRole(cashier, TestData.SuperAdmin(), TestData.Now);
        user.Grant(TestData.SlotView, TestData.SuperAdmin(), TestData.Now);
        user.Deny(TestData.CageOperate, TestData.SuperAdmin(), TestData.Now);

        var effective = user.EffectivePermissions([cashier]);

        Assert.Equal([TestData.CageView, TestData.SlotView], effective.OrderBy(c => c.ToString()));
    }

    [Fact]
    public void KK3_denied_view_disables_every_action_of_that_screen()
    {
        var cashier = TestData.NewRole();
        cashier.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);
        cashier.Grant(TestData.CageApprove, TestData.SuperAdmin(), TestData.Now);

        var user = TestData.NewUser();
        user.AssignRole(cashier, TestData.SuperAdmin(), TestData.Now);
        user.Deny(TestData.CageView, TestData.SuperAdmin(), TestData.Now);

        Assert.Empty(user.EffectivePermissions([cashier]));
    }

    [Fact]
    public void Roles_not_assigned_to_user_are_ignored()
    {
        var other = TestData.NewRole();
        other.Grant(TestData.CageView, TestData.SuperAdmin(), TestData.Now);

        Assert.Empty(TestData.NewUser().EffectivePermissions([other]));
    }
}
