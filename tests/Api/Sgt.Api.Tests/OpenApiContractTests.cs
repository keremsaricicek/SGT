using System.Text.Json;
using System.Text.Json.Nodes;

namespace Sgt.Api.Tests;

/// <summary>
/// Yayınlanan API sözleşmesi depodaki docs/api/openapi-v1.json ile aynı olmalıdır (ADR-0006).
/// Bilinçli değişiklikte SGT_UPDATE_OPENAPI=1 ile bir kez çalıştırıp dosyayı güncelleyin; kırıcıysa yeni sürüm açın.
/// </summary>
public sealed class OpenApiContractTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Published_contract_matches_committed_document()
    {
        var live = Normalize(await api.CreateClient().GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative)));
        var path = Path.Combine(RepositoryRoot(), "docs", "api", "openapi-v1.json");

        if (Environment.GetEnvironmentVariable("SGT_UPDATE_OPENAPI") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllTextAsync(path, live);
        }

        Assert.True(File.Exists(path), $"Sözleşme dosyası yok: {path}");
        Assert.True(live == Normalize(await File.ReadAllTextAsync(path)),
            "API sözleşmesi değişti: SGT_UPDATE_OPENAPI=1 ile docs/api/openapi-v1.json güncelleyin (kırıcıysa yeni sürüm).");
    }

    private static string Normalize(string json)
    {
        var node = JsonNode.Parse(json)!;
        node["servers"]?.AsArray().Clear();
        return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
    }

    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Sgt.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Depo kökü bulunamadı.");
    }
}
