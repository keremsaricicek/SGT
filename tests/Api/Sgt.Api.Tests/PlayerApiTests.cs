using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.CasinoPlayer.Application.Search;

namespace Sgt.Api.Tests;

public sealed class PlayerApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    private static object NewPlayer(string document, string lastName = "Testoğlu") => new
    {
        firstName = "Deneme", lastName, birthDate = "1990-05-01", nationality = "cy",
        documentType = "Passport", documentNumber = document, phone = "+905330000001", reasonCode = "NEW_PLAYER",
    };

    [Fact]
    public async Task Register_issue_card_and_find_by_number_card_document_and_name()
    {
        var desk = api.ClientFor(ApiFactory.SuperAdmin);
        await GrantPlayerScreens(desk);

        var created = await desk.PostAsJsonAsync("/api/v1/players", NewPlayer("PX-123456"));
        var player = (await created.Content.ReadFromJsonAsync<PlayerRegistered>())!;
        await desk.PostAsJsonAsync($"/api/v1/players/{player.PlayerId}/cards", new { cardNumber = "880011", reasonCode = "NEW_CARD" });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        foreach (var term in new[] { player.PlayerNumber, "880011", "px 123456", "Testo" })
        {
            var found = await desk.GetFromJsonAsync<List<PlayerSummary>>($"/api/v1/players?q={Uri.EscapeDataString(term)}");
            Assert.Contains(found!, p => p.PlayerId == player.PlayerId && p.ActiveCardNumber == "880011");
        }

        var view = await desk.GetFromJsonAsync<PlayerView>($"/api/v1/players/{player.PlayerId}");
        Assert.Equal("****3456", view!.MaskedDocumentNumber);
        Assert.Equal("CY", view.Nationality);
    }

    [Fact]
    public async Task Personal_data_is_encrypted_at_rest()
    {
        var desk = api.ClientFor(ApiFactory.SuperAdmin);
        await GrantPlayerScreens(desk);
        await desk.PostAsJsonAsync("/api/v1/players", NewPlayer("SECRET987654"));

        await using var connection = new NpgsqlConnection(api.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT count(*) FROM casino_player.players WHERE document_cipher LIKE '%987654%' OR phone_cipher LIKE '%5330000001%'", connection);

        Assert.Equal(0L, (long)(await command.ExecuteScalarAsync())!);
    }

    [Fact]
    public async Task Duplicate_document_is_conflict_and_restriction_needs_second_person()
    {
        var desk = api.ClientFor(ApiFactory.SuperAdmin);
        await GrantPlayerScreens(desk);
        var player = (await (await desk.PostAsJsonAsync("/api/v1/players", NewPlayer("DUPDOC1"))).Content.ReadFromJsonAsync<PlayerRegistered>())!;

        var duplicate = await desk.PostAsJsonAsync("/api/v1/players", NewPlayer("dup-doc-1"));
        var restriction = await desk.PostAsJsonAsync($"/api/v1/players/{player.PlayerId}/restrictions", new { type = "Banned", note = "kavga", reasonCode = "SECURITY" });
        var restrictionId = (await restriction.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        var selfRemove = await desk.PostAsJsonAsync($"/api/v1/players/{player.PlayerId}/restrictions/{restrictionId}/remove", new { reasonCode = "APPEAL" });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.Created, restriction.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, selfRemove.StatusCode);
    }

    [Fact]
    public async Task Staff_without_player_screen_cannot_search()
    {
        var staff = UserId.New();
        await api.AddUser(staff);

        var response = await api.ClientFor(staff).GetAsync(new Uri("/api/v1/players?q=abc", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Süper admin yalnızca yönetim ekranlarına otomatik sahiptir (Spec 001 KK11); oyuncu ekranları role verilir.
    private static async Task GrantPlayerScreens(HttpClient admin)
    {
        var self = ApiFactory.SuperAdmin.Value;
        var role = await admin.PostAsJsonAsync("/api/v1/identity/roles", new { name = $"Resepsiyon {Guid.NewGuid():N}", reasonCode = "TEST" });
        var roleId = (await role.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        foreach (var p in new[]
        {
            "player.search:view", "player.profile:view", "player.profile:create", "player.profile:edit", "player.profile:issue_card",
            "player.restrictions:view", "player.restrictions:add", "player.restrictions:remove",
        })
        {
            await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = p, reasonCode = "TEST" });
        }

        await admin.PostAsJsonAsync($"/api/v1/identity/users/{self}/roles", new { roleId, reasonCode = "TEST" });
    }

    private sealed record IdResponse(Guid Id);
}
