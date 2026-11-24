using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>Bayrağın değerlendirmesi: ya çalıştırma seçenekleri ya da ret gerekçesi.</summary>
public sealed record DemoSeedDecision(DemoSeedOptions? Options, string? Refusal);

/// <summary>
/// API'nin <c>--seed-demo</c> komutu (Spec 012 KK2, contracts/demo-cli.md): demo verisini yükleyip çıkar.
/// Çıkış kodları: 0 başarı, 1 hata, 2 reddedildi. Yalnızca izin listesindeki ortamda (Development, Demo),
/// <c>DemoSeed:Enabled=true</c> ve açıkça verilmiş <c>DemoSeed:TenantId</c> ile çalışır; varsayılan tenant yoktur.
/// </summary>
public static partial class DemoSeedCommand
{
    public const string Flag = "--seed-demo";
    public const int SuccessExitCode = 0;
    public const int FailedExitCode = 1;
    public const int RefusedExitCode = 2;

    private static readonly string[] AllowedEnvironments = ["Development", "Demo"];

    public static bool IsRequested(IEnumerable<string> args) => args.Contains(Flag, StringComparer.Ordinal);

    /// <summary>Ortam, etkinleştirme ve tenant koşullarını denetler; zaman çizelgesi gerçek andan kurulur.</summary>
    public static DemoSeedDecision Evaluate(string environmentName, IConfiguration configuration, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (!AllowedEnvironments.Contains(environmentName, StringComparer.OrdinalIgnoreCase))
        {
            return Refused($"--seed-demo yalnızca {string.Join(" / ", AllowedEnvironments)} ortamında çalışır; bu ortam: '{environmentName}'.");
        }

        var section = configuration.GetSection("DemoSeed");
        if (!bool.TryParse(section["Enabled"], out var enabled) || !enabled)
        {
            return Refused("Demo verisi kapalı: DemoSeed:Enabled=true verilmeli.");
        }

        if (!Guid.TryParse(section["TenantId"], out var tenant))
        {
            return Refused("Demo tenant'ı belirtilmeli: DemoSeed:TenantId (GUID).");
        }

        var scale = DemoScale.Parse(section["Scale"]);
        return new DemoSeedDecision(new DemoSeedOptions(new TenantId(tenant), scale, DemoTimeline.Ending(now, scale)), null);
    }

    public static int Refuse(ILogger logger, string reason)
    {
        LogRefused(logger, reason);
        return RefusedExitCode;
    }

    /// <summary>Üreticiyi çalıştırır; her hata loglanır ve 1 döner.</summary>
    public static async Task<int> Run(IServiceProvider services, DemoSeedOptions options, ILogger logger, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        await using var scope = services.CreateAsyncScope();
        try
        {
            await scope.ServiceProvider.GetRequiredService<DemoDataGenerator>().Run(options, ct);
            return SuccessExitCode;
        }
#pragma warning disable CA1031 // Komut satırı aracı: her hata loglanır ve sözleşmedeki çıkış kodu 1 döner (contracts/demo-cli.md).
        catch (Exception e) when (e is not OperationCanceledException)
#pragma warning restore CA1031
        {
            LogFailed(logger, e);
            return FailedExitCode;
        }
    }

    private static DemoSeedDecision Refused(string reason) => new(null, reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "--seed-demo reddedildi: {Reason}")]
    private static partial void LogRefused(ILogger logger, string reason);

    [LoggerMessage(Level = LogLevel.Error, Message = "Demo verisi yüklenemedi.")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
