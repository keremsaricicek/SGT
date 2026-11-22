using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>
/// API'nin <c>--seed-demo</c> komutu (Spec 012 KK2, contracts/demo-cli.md): demo verisini yükleyip çıkar.
/// Çıkış kodları: 0 başarı, 1 hata, 2 reddedildi (Production).
/// </summary>
public static partial class DemoSeedCommand
{
    public const string Flag = "--seed-demo";
    public const int SuccessExitCode = 0;
    public const int FailedExitCode = 1;
    public const int RefusedExitCode = 2;

    /// <summary>Yapılandırma yoksa geliştirme tenant'ı (deploy/.env.example SGT_TENANT_ID ile aynı).</summary>
    private static readonly TenantId DefaultTenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));

    public static bool IsRequested(IEnumerable<string> args) => args.Contains(Flag, StringComparer.Ordinal);

    /// <summary>Gerçek veri ortamında sahte veri üretilmez.</summary>
    public static bool IsAllowedIn(string environmentName) =>
        !string.Equals(environmentName, Environments.Production, StringComparison.OrdinalIgnoreCase);

    /// <summary><c>DemoSeed:TenantId</c> ve <c>DemoSeed:Scale</c> (Full | Small) okunur; zaman çizelgesi gerçek andan kurulur.</summary>
    public static DemoSeedOptions Options(IConfiguration configuration, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection("DemoSeed");
        var tenant = Guid.TryParse(section["TenantId"], out var id) ? new TenantId(id) : DefaultTenant;
        var scale = DemoScale.Parse(section["Scale"]);
        return new DemoSeedOptions(tenant, scale, DemoTimeline.Ending(now, scale));
    }

    public static int Refuse(ILogger logger, string environmentName)
    {
        LogRefused(logger, environmentName);
        return RefusedExitCode;
    }

    /// <summary>Üreticiyi çalıştırır; modül kuralına takılan demo verisi 1 ile çıkar.</summary>
    public static async Task<int> Run(IServiceProvider services, DemoSeedOptions options, ILogger logger, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        await using var scope = services.CreateAsyncScope();
        try
        {
            await scope.ServiceProvider.GetRequiredService<DemoDataGenerator>().Run(options, ct);
            return SuccessExitCode;
        }
        catch (DemoSeedException e)
        {
            LogFailed(logger, e);
            return FailedExitCode;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "--seed-demo {Environment} ortamında çalıştırılamaz; demo verisi yalnızca geliştirme/demo ortamı içindir.")]
    private static partial void LogRefused(ILogger logger, string environment);

    [LoggerMessage(Level = LogLevel.Error, Message = "Demo verisi yüklenemedi. Veritabanını sıfırlayıp tekrar deneyin (build/demo.sh reset).")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
