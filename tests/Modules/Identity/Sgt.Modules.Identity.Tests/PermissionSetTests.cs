using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class PermissionSetTests
{
    [Fact]
    public void KK1_non_view_permission_requires_view_of_same_screen()
    {
        var result = PermissionSet.Empty.With(TestData.CageOperate);

        Assert.Equal(IdentityErrors.ViewRequired, result.Error);
    }

    [Fact]
    public void KK1_non_view_permission_is_allowed_after_view()
    {
        var set = PermissionSet.Empty.With(TestData.CageView).Value.With(TestData.CageOperate).Value;

        Assert.True(set.Contains(TestData.CageOperate));
    }

    [Fact]
    public void KK2_view_cannot_be_removed_while_other_actions_remain()
    {
        var set = PermissionSet.Empty.With(TestData.CageView).Value.With(TestData.CageApprove).Value;

        Assert.Equal(IdentityErrors.ViewInUse, set.Without(TestData.CageView).Error);
    }

    [Fact]
    public void KK2_view_can_be_removed_when_alone()
    {
        var set = PermissionSet.Empty.With(TestData.CageView).Value;

        Assert.False(set.Without(TestData.CageView).Value.Contains(TestData.CageView));
    }

    [Fact]
    public void Sets_are_immutable()
    {
        var original = PermissionSet.Empty;
        _ = original.With(TestData.CageView);

        Assert.Empty(original.Codes);
    }
}
