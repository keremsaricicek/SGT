using System.Net;
using System.Net.Http.Json;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Panel;

namespace Sgt.Api.Tests;

public sealed class PermissionPanelApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Request_without_identity_is_rejected()
    {
        var response = await api.CreateClient().GetAsync(new Uri("/api/v1/identity/me/menu", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Super_admin_sets_up_default_roles_and_change_history_is_visible()
    {
        var admin = api.ClientFor(ApiFactory.SuperAdmin);

        var create = await admin.PostAsJsonAsync("/api/v1/identity/roles/defaults", new { reasonCode = "INITIAL_SETUP" });
        var roles = await admin.GetFromJsonAsync<List<RoleView>>("/api/v1/identity/roles");
        var history = await admin.GetFromJsonAsync<List<PermissionChangeView>>("/api/v1/identity/permission-changes?take=5");

        Assert.Equal(HttpStatusCode.NoContent, create.StatusCode);
        Assert.Contains(roles!, r => r.Name == "Slot müdürü");
        Assert.Equal(5, history!.Count);
        Assert.All(history, c => Assert.Equal("INITIAL_SETUP", c.ReasonCode));
        Assert.All(history, c => Assert.Equal("TEST-PC", c.Terminal));
    }

    [Fact]
    public async Task KK12_change_without_reason_is_rejected()
    {
        var response = await api.ClientFor(ApiFactory.SuperAdmin)
            .PostAsJsonAsync("/api/v1/identity/roles", new { name = "Bar", reasonCode = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(ReasonCode.Required.Code, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task KK5_staff_cannot_create_roles_and_KK10_cannot_read_panel()
    {
        var staff = UserId.New();
        await api.AddUser(staff);
        var client = api.ClientFor(staff);

        var create = await client.PostAsJsonAsync("/api/v1/identity/roles", new { name = "Bar", reasonCode = "TEST" });
        var read = await client.GetAsync(new Uri("/api/v1/identity/roles", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, read.StatusCode);
    }

    [Fact]
    public async Task KK4_staff_menu_follows_assigned_role()
    {
        var admin = api.ClientFor(ApiFactory.SuperAdmin);
        var created = await admin.PostAsJsonAsync("/api/v1/identity/roles", new { name = "Slot vardiya", reasonCode = "TEST" });
        var roleId = (await created.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = "slot.floor:view", reasonCode = "TEST" });

        var staff = UserId.New();
        await api.AddUser(staff);
        var assign = await admin.PostAsJsonAsync($"/api/v1/identity/users/{staff.Value}/roles", new { roleId, reasonCode = "TEST" });
        var menu = await api.ClientFor(staff).GetFromJsonAsync<List<MenuItem>>("/api/v1/identity/me/menu");

        Assert.Equal(HttpStatusCode.NoContent, assign.StatusCode);
        Assert.Equal(["slot.floor"], menu!.Select(m => m.Screen));
    }

    [Fact]
    public async Task Invalid_permission_returns_problem_with_code()
    {
        var admin = api.ClientFor(ApiFactory.SuperAdmin);
        var created = await admin.PostAsJsonAsync("/api/v1/identity/roles", new { name = "Deneme", reasonCode = "TEST" });
        var roleId = (await created.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        var response = await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = "cage.adjustment:approve", reasonCode = "TEST" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(body.Contains("Identity.ViewRequired", StringComparison.Ordinal), body);
    }

    private sealed record IdResponse(Guid Id);
}
