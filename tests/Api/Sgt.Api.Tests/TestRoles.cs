using System.Net.Http.Json;

namespace Sgt.Api.Tests;

/// <summary>Testte süper admine ekran yetkileri veren yardımcı (yönetim dışı ekranlar role verilir).</summary>
internal static class TestRoles
{
    public static Task Grant(HttpClient admin, params string[] permissions) =>
        GrantTo(admin, ApiFactory.SuperAdmin, permissions);

    public static async Task GrantTo(HttpClient admin, Sgt.BuildingBlocks.UserId user, params string[] permissions)
    {
        var role = await admin.PostAsJsonAsync("/api/v1/identity/roles", new { name = $"Test {Guid.NewGuid():N}", reasonCode = "TEST" });
        var roleId = (await role.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        foreach (var screen in permissions.Select(p => p.Split(':')[0]).Distinct())
        {
            await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = $"{screen}:view", reasonCode = "TEST" });
        }

        foreach (var p in permissions.Where(p => !p.EndsWith(":view", StringComparison.Ordinal)))
        {
            await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = p, reasonCode = "TEST" });
        }

        await admin.PostAsJsonAsync($"/api/v1/identity/users/{user.Value}/roles", new { roleId, reasonCode = "TEST" });
    }

    private sealed record IdResponse(Guid Id);
}
