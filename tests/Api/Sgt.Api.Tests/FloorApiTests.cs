using System.Net;
using System.Net.Http.Json;
using Sgt.Modules.Audit.Application.Search;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.GamingFloor.Contracts;

namespace Sgt.Api.Tests;

public sealed class FloorApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Pit_flow_define_open_rate_locate_finish_and_audit()
    {
        var pit = api.ClientFor(ApiFactory.SuperAdmin);
        await TestRoles.Grant(pit, "player.profile:create", "live.tables:define", "live.table_session:open", "live.table_session:close",
            "live.rating:start", "live.rating:finish", "player.locator:view");
        var player = (await (await pit.PostAsJsonAsync("/api/v1/players", new
        {
            firstName = "Masa", lastName = "Oyuncu", birthDate = "1980-02-02", nationality = "GB", documentType = "Passport",
            documentNumber = $"G{Guid.NewGuid():N}"[..10], reasonCode = "NEW_PLAYER",
        })).Content.ReadFromJsonAsync<PlayerRegistered>())!;

        var table = await pit.PostAsJsonAsync("/api/v1/floor/tables", new
        {
            code = "bj-07", gameType = "Blackjack", currency = "TRY", minBet = 100m, maxBet = 10_000m, houseEdgePercent = 2m, handsPerHour = 70,
            reasonCode = "SETUP",
        });
        var tableId = (await table.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        await pit.PostAsJsonAsync($"/api/v1/floor/tables/{tableId}/open", new { reasonCode = "OPEN" });

        var correlation = Guid.NewGuid();
        pit.DefaultRequestHeaders.Add("X-Correlation-Id", correlation.ToString());
        var rating = await pit.PostAsJsonAsync("/api/v1/floor/ratings", new { tableId, playerId = player.PlayerId, reasonCode = "RATE" });
        var ratingId = (await rating.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        var location = await pit.GetFromJsonAsync<PlayerLocationDto>($"/api/v1/floor/players/{player.PlayerId}/location");
        var closeWhileRunning = await pit.PostAsJsonAsync($"/api/v1/floor/tables/{tableId}/close", new { reasonCode = "CLOSE" });
        var finish = await pit.PostAsJsonAsync($"/api/v1/floor/ratings/{ratingId}/finish", new { averageBet = 500m, playerResult = 2_000m, reasonCode = "RATE" });
        var close = await pit.PostAsJsonAsync($"/api/v1/floor/tables/{tableId}/close", new { reasonCode = "CLOSE" });

        Assert.Equal(HttpStatusCode.Created, table.StatusCode);
        Assert.Equal(HttpStatusCode.Created, rating.StatusCode);
        Assert.Equal("BJ-07", location!.TableCode);
        Assert.Equal(HttpStatusCode.Conflict, closeWhileRunning.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, finish.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, close.StatusCode);

        var deadline = DateTime.UtcNow.AddSeconds(15);
        List<AuditEntryView>? entries;
        do
        {
            await Task.Delay(200);
            entries = await pit.GetFromJsonAsync<List<AuditEntryView>>($"/api/v1/audit/entries?correlationId={correlation}");
        }
        while (entries!.Count < 2 && DateTime.UtcNow < deadline);

        Assert.Contains(entries, e => e.EventType == typeof(PlayerRatedV1).FullName);
    }

    private sealed record IdResponse(Guid Id);
}
