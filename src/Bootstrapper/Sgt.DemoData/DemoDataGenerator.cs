using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.Configuration.Application.Markers;

namespace Sgt.DemoData;

/// <summary>
/// Demo verisini üretir (Spec 012). Sıra: tenant kilidi → tamamlanma işareti varsa hiçbir şey yazmaz (KK5) →
/// işaretsiz veri varsa durur (yarım yükleme, reset önerilir) → tenant ön koşulu → adımlar → outbox boş mu →
/// tamamlanma işareti (son adım).
/// </summary>
public sealed partial class DemoDataGenerator(
    IEnumerable<IDemoStep> steps,
    DemoCommands commands,
    DemoOutbox outbox,
    IDemoTenantBootstrap bootstrap,
    IAdvisoryLock locks,
    DemoClock clock,
    ILogger<DemoDataGenerator> logger)
{
    /// <summary>Demo verisinin tamamı yazıldı (Configuration modülünde tenant işareti).</summary>
    public const string CompletionMarker = "DEMO_DATA_V1";

    private const string FirstPlayerNumber = "P000001";

    public static string LockName(TenantId tenant) => $"sgt:demo-seed:{tenant.Value}";

    public async Task<DemoSeedResult> Run(DemoSeedOptions options, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);
        var total = Stopwatch.StartNew();
        await using var held = await locks.TryAcquire(LockName(options.Tenant), ct)
            ?? throw new DemoSeedException($"Tenant {options.Tenant.Value} için başka bir demo yüklemesi sürüyor.");

        if (await commands.Query<IsTenantMarkerSetQuery, bool>(new IsTenantMarkerSetQuery(options.Tenant, CompletionMarker), ct))
        {
            LogAlreadySeeded(logger, options.Tenant.Value);
            return new DemoSeedResult(false, total.Elapsed);
        }

        var players = await commands.Query<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>(new SearchPlayersQuery(options.Tenant, FirstPlayerNumber), ct);
        if (players.Count > 0)
        {
            throw new DemoSeedException(
                $"Tenant {options.Tenant.Value} içinde veri var ama demo yüklemesi tamamlanmamış; reset önerilir (build/demo.sh reset --yes).");
        }

        LogStarting(logger, options.Tenant.Value, options.Scale.Name, options.Timeline.FirstDay, options.Timeline.Today);
        clock.Set(options.Timeline.SetupStart);
        await bootstrap.Prepare(options.Tenant, DemoUsers.SuperAdmin.Id, ct);

        var run = new DemoRun(options, clock);
        foreach (var step in steps.OrderBy(s => s.Order))
        {
            var watch = Stopwatch.StartNew();
            await step.Run(run, ct);
            LogStepDone(logger, step.Name, watch.Elapsed.TotalSeconds);
        }

        // Simülasyon bitti: saat gerçek ana gelir (kalan kayıtlar ve işaret "şimdi" tarihlidir).
        if (clock.UtcNow < options.Timeline.Now)
        {
            clock.Set(options.Timeline.Now);
        }

        await outbox.DrainAll(ct);
        await outbox.EnsureEmpty(options.Tenant, ct);
        await commands.Do<ICommandHandler<SetTenantMarkerCommand>>(
            "tamamlanma işareti", h => h.Handle(new SetTenantMarkerCommand(options.Tenant, CompletionMarker, run.Audit(DemoUsers.SuperAdmin.Id)), ct));

        // İşaretin kendi denetim olayı.
        await outbox.DrainAll(ct);
        await outbox.EnsureEmpty(options.Tenant, ct);

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
