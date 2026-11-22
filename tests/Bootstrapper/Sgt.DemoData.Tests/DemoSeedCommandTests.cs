using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Sgt.DemoData.Tests;

public sealed class DemoSeedCommandTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(new[] { "--seed-demo" }, true)]
    [InlineData(new[] { "--migrate" }, false)]
    [InlineData(new string[0], false)]
    public void Flag_is_detected(string[] args, bool expected)
    {
        Assert.Equal(expected, DemoSeedCommand.IsRequested(args));
    }

    [Theory]
    [InlineData("Production", false)]
    [InlineData("Development", true)]
    [InlineData("Demo", true)]
    public void KK2_flag_is_refused_in_production(string environment, bool allowed)
    {
        Assert.Equal(allowed, DemoSeedCommand.IsAllowedIn(environment));
        Assert.Equal(2, DemoSeedCommand.RefusedExitCode);
    }

    [Fact]
    public void Options_default_to_development_tenant_and_full_scale()
    {
        var options = DemoSeedCommand.Options(new ConfigurationBuilder().Build(), Now);

        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-0000000000a1"), options.Tenant.Value);
        Assert.Equal(DemoScale.Full, options.Scale);
        Assert.Equal(new DateOnly(2026, 11, 21), options.Timeline.Today);
        Assert.Equal(30, options.Timeline.Days);
    }

    [Fact]
    public void Options_read_demo_seed_section()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DemoSeed:TenantId"] = "11111111-1111-1111-1111-111111111111",
            ["DemoSeed:Scale"] = "Small",
        }).Build();

        var options = DemoSeedCommand.Options(config, Now);

        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), options.Tenant.Value);
        Assert.Equal(DemoScale.Small, options.Scale);
    }

    [Fact]
    public void Production_name_matches_host_constant()
    {
        Assert.False(DemoSeedCommand.IsAllowedIn(Environments.Production));
    }
}
