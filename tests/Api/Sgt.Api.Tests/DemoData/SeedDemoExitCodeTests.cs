using System.Diagnostics;

namespace Sgt.Api.Tests.DemoData;

/// <summary>Spec 012 KK2: --seed-demo yalnızca Development/Demo ortamında, açıkça etkinleştirilmiş ve tenant verilmişken çalışır; aksi 2 ile çıkar.</summary>
public sealed class SeedDemoExitCodeTests
{
    private const string Tenant = "00000000-0000-0000-0000-0000000000a1";

    [Theory]
    [InlineData("Production", "true", Tenant)]
    [InlineData("prod", "true", Tenant)]
    [InlineData("Staging", "true", Tenant)]
    [InlineData("Development", "false", Tenant)]
    [InlineData("Development", "true", "")]
    public async Task KK2_refused_runs_exit_with_2_before_touching_the_database(string environment, string enabled, string tenant)
    {
        var code = await RunApi(environment, enabled, tenant);

        Assert.Equal(2, code);
    }

    private static async Task<int> RunApi(string environment, string enabled, string tenant)
    {
        var dll = typeof(Program).Assembly.Location;
        var start = new ProcessStartInfo(Environment.ProcessPath!.EndsWith("dotnet", StringComparison.Ordinal) ? Environment.ProcessPath : "dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(dll)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add(dll);
        start.ArgumentList.Add("--seed-demo");
        start.Environment["ASPNETCORE_ENVIRONMENT"] = environment;
        start.Environment["DemoSeed__Enabled"] = enabled;
        start.Environment["DemoSeed__TenantId"] = tenant;
        // Veritabanına gidilirse test başarısız olsun: erişilemez adres.
        start.Environment["ConnectionStrings__Sgt"] = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=2";

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await process.WaitForExitAsync(timeout.Token);
        await Task.WhenAll(output, errors);
        return process.ExitCode;
    }
}
