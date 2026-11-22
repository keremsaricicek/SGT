using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sgt.Modules.CasinoPlayer.Application.Search;

namespace Sgt.DemoData;

/// <summary>
/// Demo verisini üretir (Spec 012): adımları sırayla çalıştırır, sonra saati gerçek ana getirir.
/// Tenant'ta demo oyuncusu varsa hiçbir şey yazmaz (KK5).
/// </summary>
public sealed partial class DemoDataGenerator(
    IEnumerable<IDemoStep> steps, DemoCommands commands, DemoClock clock, ILogger<DemoDataGenerator> logger)
{
    private const string FirstPlayerNumber = "P000001";

    public async Task<DemoSeedResult> Run(DemoSeedOptions options, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);
        var total = Stopwatch.StartNew();
        var existing = await commands.Query<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>(new SearchPlayersQuery(options.Tenant, FirstPlayerNumber), ct);
        if (existing.Count > 0)
        {
            LogAlreadySeeded(logger, options.Tenant.Value);
            return new DemoSeedResult(false, total.Elapsed);
        }

        LogStarting(logger, options.Tenant.Value, options.Scale.Name, options.Timeline.FirstDay, options.Timeline.Today);
        clock.Set(options.Timeline.SetupStart);
        var run = new DemoRun(options, clock);
        foreach (var step in steps.OrderBy(s => s.Order))
        {
            var watch = Stopwatch.StartNew();
            await step.Run(run, ct);
            LogStepDone(logger, step.Name, watch.Elapsed.TotalSeconds);
        }

        LogFinished(logger, total.Elapsed.TotalSeconds);
        return new DemoSeedResult(true, total.Elapsed);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo verisi zaten var (tenant {Tenant}); hiçbir şey yazılmadı.")]
    private static partial void LogAlreadySeeded(ILogger logger, Guid tenant);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo verisi üretiliyor: tenant {Tenant}, ölçek {Scale}, {FirstDay} – {Today}")]
    private static partial void LogStarting(ILogger logger, Guid tenant, string scale, DateOnly firstDay, DateOnly today);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo adımı bitti: {Step} ({Seconds:F1} sn)")]
    private static partial void LogStepDone(ILogger logger, string step, double seconds);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo verisi hazır ({Seconds:F1} sn).")]
    private static partial void LogFinished(ILogger logger, double seconds);
}
