namespace Sgt.DemoData.Tests;

public sealed class DemoScaleTests
{
    [Fact]
    public void Full_scale_matches_spec_volumes()
    {
        Assert.Equal(new DemoScale("Full", 500, 40, 200, 30), DemoScale.Full);
    }

    [Theory]
    [InlineData("Full", 500)]
    [InlineData("small", 20)]
    [InlineData(null, 500)]
    [InlineData("", 500)]
    public void Parse_reads_configuration_value(string? value, int players)
    {
        Assert.Equal(players, DemoScale.Parse(value).Players);
    }

    [Fact]
    public void Parse_rejects_unknown_scale()
    {
        Assert.Throws<ArgumentException>(() => DemoScale.Parse("Huge"));
    }
}
