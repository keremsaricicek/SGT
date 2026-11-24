using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

public sealed class PermissionCodeTests
{
    [Fact]
    public void Parse_splits_screen_and_action()
    {
        var code = TestData.Code("Cage.Adjustment:Approve");

        Assert.Equal("cage.adjustment", code.Screen);
        Assert.Equal("approve", code.Action);
        Assert.Equal("cage.adjustment:approve", code.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("cage.adjustment")]
    [InlineData(":view")]
    [InlineData("cage.adjustment:")]
    [InlineData("cage adjustment:view")]
    public void Parse_rejects_malformed_codes(string value)
    {
        Assert.Equal(IdentityErrors.InvalidPermissionCode, PermissionCode.Parse(value).Error);
    }

    [Fact]
    public void ViewOf_returns_the_view_code_of_the_same_screen()
    {
        Assert.Equal(TestData.CageView, TestData.CageApprove.ViewOfScreen());
        Assert.True(TestData.CageView.IsView);
    }
}
