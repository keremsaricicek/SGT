using System.Text.Json;

namespace Sgt.DemoData.Tests;

/// <summary>
/// Spec 012 KK6: demo kullanıcıları SGT'deki sabit kimliklerle aynıdır. Pilot kurulumun realm'i yalnızca superadmin ve
/// kasiyer1'i içerir; diğer demo kullanıcıları ayrı dosyadadır ve yalnızca build/demo.sh yükler. Şifreler repoda değildir.
/// </summary>
public sealed class RealmUsersTests
{
    private static readonly string[] RealmUsers = ["superadmin", "kasiyer1"];

    [Fact]
    public void Pilot_realm_contains_only_base_users_with_env_passwords()
    {
        var users = Users(Read("sgt-realm.json"));

        Assert.Equal(RealmUsers.Order(StringComparer.Ordinal), users.Keys.Order(StringComparer.Ordinal));
        foreach (var demo in DemoUsers.All.Where(u => RealmUsers.Contains(u.UserName)))
        {
            AssertUser(demo, users[demo.UserName], passwordPrefix: "${SGT_");
        }
    }

    [Fact]
    public void Demo_users_file_holds_the_other_staff_with_same_ids_and_placeholder_password()
    {
        using var doc = Read("demo-users.json");
        var users = Users(doc);

        Assert.Equal("SKIP", doc.RootElement.GetProperty("ifResourceExists").GetString());
        var expected = DemoUsers.All.Where(u => !RealmUsers.Contains(u.UserName)).ToList();
        Assert.Equal(expected.Select(u => u.UserName).Order(StringComparer.Ordinal), users.Keys.Order(StringComparer.Ordinal));
        foreach (var demo in expected)
        {
            AssertUser(demo, users[demo.UserName], passwordPrefix: "__SGT_DEMO_PASSWORD__");
        }
    }

    private static void AssertUser(DemoUser demo, JsonElement user, string passwordPrefix)
    {
        Assert.Equal(demo.Id.Value, user.GetProperty("id").GetGuid());
        Assert.Equal("00000000-0000-0000-0000-0000000000a1", user.GetProperty("attributes").GetProperty("tenant_id")[0].GetString());
        Assert.StartsWith(passwordPrefix, user.GetProperty("credentials")[0].GetProperty("value").GetString(), StringComparison.Ordinal);
    }

    private static Dictionary<string, JsonElement> Users(JsonDocument doc) =>
        doc.RootElement.GetProperty("users").EnumerateArray().ToDictionary(u => u.GetProperty("username").GetString()!, u => u.Clone());

    private static JsonDocument Read(string file) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(RepoRoot(), "deploy", "keycloak", file)));

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
