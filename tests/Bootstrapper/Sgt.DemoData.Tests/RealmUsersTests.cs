using System.Text.Json;

namespace Sgt.DemoData.Tests;

/// <summary>Spec 012 KK6: Keycloak realm'deki demo kullanıcıları SGT'deki sabit kimliklerle aynıdır; şifre repoda değil env'dedir.</summary>
public sealed class RealmUsersTests
{
    [Fact]
    public void Every_demo_user_exists_in_realm_with_same_sub_and_env_password()
    {
        using var realm = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "deploy", "keycloak", "sgt-realm.json")));
        var users = realm.RootElement.GetProperty("users").EnumerateArray()
            .ToDictionary(u => u.GetProperty("username").GetString()!, u => u);

        foreach (var demo in DemoUsers.All)
        {
            var user = users[demo.UserName];
            Assert.Equal(demo.Id.Value, user.GetProperty("id").GetGuid());
            Assert.Equal("00000000-0000-0000-0000-0000000000a1", user.GetProperty("attributes").GetProperty("tenant_id")[0].GetString());
            var password = user.GetProperty("credentials")[0].GetProperty("value").GetString()!;
            Assert.StartsWith("${SGT_", password, StringComparison.Ordinal);
        }
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Sgt.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Repo kökü bulunamadı.");
    }
}
