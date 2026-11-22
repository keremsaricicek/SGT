namespace Sgt.DemoData.Steps;

/// <summary>TRY bazlı EUR/GBP/USD başlangıç kurları (Spec 012 KK3). Kurlar her simüle günün sabahı yenilenir.</summary>
public sealed class ConfigurationSeeder(RateBoard rates) : IDemoStep
{
    public int Order => 20;

    public string Name => "Kurlar";

    public Task Run(DemoRun run, CancellationToken ct) => rates.Publish(run, maxDrift: 0, ct);
}
