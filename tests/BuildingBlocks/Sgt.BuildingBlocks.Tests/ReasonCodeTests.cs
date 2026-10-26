using Sgt.BuildingBlocks;

namespace Sgt.BuildingBlocks.Tests;

public sealed class ReasonCodeTests
{
    [Fact]
    public void Create_normalizes_to_upper_case()
    {
        Assert.Equal("ROLE_SETUP", ReasonCode.Create(" role_setup ").Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_missing_reason(string? value)
    {
        Assert.Equal(ReasonCode.Required, ReasonCode.Create(value).Error);
    }

    [Fact]
    public void Create_rejects_overlong_reason()
    {
        Assert.True(ReasonCode.Create(new string('A', 51)).IsFailure);
    }
}
