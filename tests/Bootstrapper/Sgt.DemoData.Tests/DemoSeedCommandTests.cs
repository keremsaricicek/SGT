using Microsoft.Extensions.Configuration;

namespace Sgt.DemoData.Tests;

public sealed class DemoSeedCommandTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 10, 0, 0, TimeSpan.Zero);
    private const string Tenant = "11111111-1111-1111-1111-111111111111";

    [Theory]
    [InlineData(new[] { "--seed-demo" }, true)]
    [InlineData(new[] { "--migrate" }, false)]
    [InlineData(new string[0], false)]
    public void Flag_is_detected(string[] args, bool expected)
    {
        Assert.Equal(expected, DemoSeedCommand.IsRequested(args));
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("prod")]
    [InlineData("Staging")]
    [InlineData("Testing")]
    [InlineData("")]
    public void KK2_only_development_or_demo_environment_is_allowed(string environment)
    {
        var decision = DemoSeedCommand.Evaluate(environment, Config(enabled: "true", tenant: Tenant), Now);

        Assert.Null(decision.Options);
        Assert.Contains("ortam", decision.Refusal, StringComparison.Ordinal);
        Assert.Equal(2, DemoSeedCommand.RefusedExitCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("false")]
    [InlineData("yes")]
    public void KK2_demo_seed_must_be_enabled_explicitly(string? enabled)
    {
        var decision = DemoSeedCommand.Evaluate("Development", Config(enabled, Tenant), Now);

        Assert.Null(decision.Options);
        Assert.Contains("DemoSeed:Enabled", decision.Refusal, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-guid")]
    public void KK2_tenant_must_be_given_explicitly(string? tenant)
    {
        var decision = DemoSeedCommand.Evaluate("Demo", Config("true", tenant), Now);

        Assert.Null(decision.Options);
        Assert.Contains("DemoSeed:TenantId", decision.Refusal, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Demo")]
    public void Allowed_environment_with_enabled_flag_and_tenant_yields_options(string environment)
    {
        var decision = DemoSeedCommand.Evaluate(environment, Config("true", Tenant, scale: "Small"), Now);

        Assert.Null(decision.Refusal);
        Assert.Equal(Guid.Parse(Tenant), decision.Options!.Tenant.Value);
        Assert.Equal(DemoScale.Small, decision.Options.Scale);
        Assert.Equal(new DateOnly(2026, 11, 21), decision.Options.Timeline.Today);
        Assert.Equal(3, decision.Options.Timeline.Days);
    }

    private static IConfiguration Config(string? enabled, string? tenant, string? scale = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DemoSeed:Enabled"] = enabled,
            ["DemoSeed:TenantId"] = tenant,
            ["DemoSeed:Scale"] = scale,
        }).Build();
}
