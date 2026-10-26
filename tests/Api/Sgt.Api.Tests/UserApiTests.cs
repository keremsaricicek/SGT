using System.Net;
using System.Net.Http.Json;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Panel;

namespace Sgt.Api.Tests;

public sealed class UserApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Admin_registers_staff_who_loses_access_when_deactivated()
    {
        var admin = api.ClientFor(ApiFactory.SuperAdmin);
        var staff = Guid.NewGuid();

        var register = await admin.PostAsJsonAsync("/api/identity/users",
            new { userId = staff, displayName = "Deneme Personel", employeeCode = "T-100", reasonCode = "HIRE" });
        var users = await admin.GetFromJsonAsync<List<UserSummary>>("/api/identity/users");

        var created = await admin.PostAsJsonAsync("/api/identity/roles", new { name = "Raporcu", reasonCode = "TEST" });
        var roleId = (await created.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        await admin.PostAsJsonAsync($"/api/identity/roles/{roleId}/permissions", new { permission = "report.daily:view", reasonCode = "TEST" });
        await admin.PostAsJsonAsync($"/api/identity/users/{staff}/roles", new { roleId, reasonCode = "TEST" });

        var staffClient = api.ClientFor(new UserId(staff));
        var menuBefore = await staffClient.GetFromJsonAsync<List<MenuItem>>("/api/identity/me/menu");
        var deactivate = await admin.PostAsJsonAsync($"/api/identity/users/{staff}/deactivate", new { reasonCode = "LEFT_COMPANY" });
        var menuAfter = await staffClient.GetFromJsonAsync<List<MenuItem>>("/api/identity/me/menu");

        Assert.Equal(HttpStatusCode.NoContent, register.StatusCode);
        Assert.Contains(users!, u => u.UserId == staff && u.EmployeeCode == "T-100" && u.IsActive);
        Assert.Equal(["report.daily"], menuBefore!.Select(m => m.Screen));
        Assert.Equal(HttpStatusCode.NoContent, deactivate.StatusCode);
        Assert.Empty(menuAfter!);
    }

    [Fact]
    public async Task Duplicate_employee_code_is_a_conflict()
    {
        var admin = api.ClientFor(ApiFactory.SuperAdmin);
        await admin.PostAsJsonAsync("/api/identity/users", new { userId = Guid.NewGuid(), displayName = "A", employeeCode = "DUP-1", reasonCode = "HIRE" });

        var second = await admin.PostAsJsonAsync("/api/identity/users", new { userId = Guid.NewGuid(), displayName = "B", employeeCode = "dup-1", reasonCode = "HIRE" });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Last_super_admin_cannot_be_deactivated()
    {
        var response = await api.ClientFor(ApiFactory.SuperAdmin)
            .PostAsJsonAsync($"/api/identity/users/{ApiFactory.SuperAdmin.Value}/deactivate", new { reasonCode = "TEST" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private sealed record IdResponse(Guid Id);
}
