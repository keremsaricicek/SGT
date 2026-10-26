using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.Audit.Application.Search;

namespace Sgt.Api.Tests;

public sealed class AuditApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task KK9_permission_change_made_through_api_appears_in_audit_log()
    {
        var correlationId = Guid.NewGuid();
        var admin = api.ClientFor(ApiFactory.SuperAdmin);
        admin.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId.ToString());

        var created = await admin.PostAsJsonAsync("/api/identity/roles", new { name = "Denetim deneme", reasonCode = "AUDIT_TEST" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var entries = await WaitForEntries(admin, correlationId);

        var entry = Assert.Single(entries);
        Assert.Equal("Sgt.Modules.Identity.Contracts.PermissionChangedV1", entry.EventType);
        Assert.Equal(ApiFactory.SuperAdmin.Value, entry.ActorId);
        Assert.Equal("TEST-PC", entry.Terminal);
        Assert.Equal("AUDIT_TEST", entry.ReasonCode);
        Assert.Contains("RoleCreated Denetim deneme", entry.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public async Task KK7_audit_entries_cannot_be_changed_or_deleted()
    {
        await using var connection = new NpgsqlConnection(api.ConnectionString);
        await connection.OpenAsync();
        await using var insert = new NpgsqlCommand(
            "INSERT INTO audit.audit_entries (id, tenant_id, event_type, summary, actor_id, terminal, reason_code, correlation_id, occurred_at, recorded_at) " +
            "VALUES (gen_random_uuid(), gen_random_uuid(), 'T', 'S', gen_random_uuid(), 'PC', 'R', gen_random_uuid(), now(), now())",
            connection);
        await insert.ExecuteNonQueryAsync();

        await using var update = new NpgsqlCommand("UPDATE audit.audit_entries SET summary = 'x'", connection);
        await using var delete = new NpgsqlCommand("DELETE FROM audit.audit_entries", connection);

        await Assert.ThrowsAsync<PostgresException>(() => update.ExecuteNonQueryAsync());
        await Assert.ThrowsAsync<PostgresException>(() => delete.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task KK8_staff_without_permission_cannot_search_audit_log()
    {
        var staff = UserId.New();
        await api.AddUser(staff);

        var response = await api.ClientFor(staff).GetAsync(new Uri("/api/audit/entries", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task KK5_operator_sees_dead_letters_and_unknown_reprocess_is_not_found()
    {
        var admin = api.ClientFor(ApiFactory.SuperAdmin);

        var list = await admin.GetAsync(new Uri("/api/integration/dead-letters", UriKind.Relative));
        var reprocess = await admin.PostAsync(new Uri($"/api/integration/dead-letters/identity/{Guid.NewGuid()}/reprocess", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, reprocess.StatusCode);
    }

    private static async Task<List<AuditEntryView>> WaitForEntries(HttpClient client, Guid correlationId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            var entries = await client.GetFromJsonAsync<List<AuditEntryView>>($"/api/audit/entries?correlationId={correlationId}");
            if (entries is { Count: > 0 } || DateTime.UtcNow > deadline)
            {
                return entries ?? [];
            }

            await Task.Delay(200);
        }
    }
}
