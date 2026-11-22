using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Sgt.DemoData;

/// <summary>
/// Demo verisini üretir (Spec 012): ön denetim ve başlangıç (<see cref="DemoSeedGate"/>) → adımlar sırayla →
/// bitiş ve tamamlanma işareti (<see cref="DemoCompletion"/>). Tamamlanmış tenant'ta hiçbir şey yazmaz (KK5).
/// </summary>
public sealed partial class DemoDataGenerator(
    IEnumerable<IDemoStep> steps, DemoSeedGate gate, DemoCompletion completion, DemoClock clock, ILogger<DemoDataGenerator> logger)
{
    public async Task<DemoSeedResult> Run(DemoSeedOptions options, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);
        var total = Stopwatch.StartNew();
        var run = new DemoRun(options, clock);

        // İşaretler denetlenmeden saat değişmez: tamamlanmış tenant'ta saat gerçek andadır, geri alınamaz.
        await using var permit = await gate.Enter(options, run.Audit(DemoUsers.SuperAdmin.Id), ct);
        if (permit is null)
        {
            LogAlreadySeeded(logger, options.Tenant.Value);
            return new DemoSeedResult(false, total.Elapsed);
        }

        LogStarting(logger, options.Tenant.Value, options.Scale.Name, options.Timeline.FirstDay, options.Timeline.Today);
        foreach (var step in steps.OrderBy(s => s.Order))
        {
            var watch = Stopwatch.StartNew();
            await step.Run(run, ct);
            LogStepDone(logger, step.Name, watch.Elapsed.TotalSeconds);
        }

        await completion.Complete(options, run.Audit(DemoUsers.SuperAdmin.Id), ct);
        LogFinished(logger, total.Elapsed.TotalSeconds);
        return new DemoSeedResult(true, total.Elapsed);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo verisi zaten tamamlanmış (tenant {Tenant}); hiçbir şey yazılmadı.")]
    private static partial void LogAlreadySeeded(ILogger logger, Guid tenant);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo verisi üretiliyor: tenant {Tenant}, ölçek {Scale}, {FirstDay} – {Today}")]
    private static partial void LogStarting(ILogger logger, Guid tenant, string scale, DateOnly firstDay, DateOnly today);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo adımı bitti: {Step} ({Seconds:F1} sn)")]
    private static partial void LogStepDone(ILogger logger, string step, double seconds);

    [LoggerMessage(Level = LogLevel.Information, Message = "Demo verisi hazır ({Seconds:F1} sn).")]
    private static partial void LogFinished(ILogger logger, double seconds);
}
