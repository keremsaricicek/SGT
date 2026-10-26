using System.Net;
using System.Net.Http.Json;
using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.Loyalty.Application.Summary;

namespace Sgt.Api.Tests;

public sealed class LoyaltyApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Rating_earns_points_and_comp_needs_second_person()
    {
        var host = api.ClientFor(ApiFactory.SuperAdmin);
        await TestRoles.Grant(host, "player.profile:create", "live.tables:define", "live.table_session:open", "live.rating:start", "live.rating:finish",
            "loyalty.points:view", "loyalty.comp:request", "loyalty.comp:approve");
        var player = (await (await host.PostAsJsonAsync("/api/v1/players", new
        {
            firstName = "Puan", lastName = "Kazanan", birthDate = "1975-03-03", nationality = "TR", documentType = "NationalId",
            documentNumber = $"L{Guid.NewGuid():N}"[..11], reasonCode = "NEW_PLAYER",
        })).Content.ReadFromJsonAsync<PlayerRegistered>())!;

        var tableId = (await (await host.PostAsJsonAsync("/api/v1/floor/tables", new
        {
            code = $"RL-{Guid.NewGuid():N}"[..8], gameType = "Roulette", currency = "TRY", minBet = 100m, maxBet = 100_000m, houseEdgePercent = 100m, handsPerHour = 1000,
            reasonCode = "SETUP",
        })).Content.ReadFromJsonAsync<IdResponse>())!.Id;
        await host.PostAsJsonAsync($"/api/v1/floor/tables/{tableId}/open", new { reasonCode = "OPEN" });
        var ratingId = (await (await host.PostAsJsonAsync("/api/v1/floor/ratings", new { tableId, playerId = player.PlayerId, reasonCode = "RATE" }))
            .Content.ReadFromJsonAsync<IdResponse>())!.Id;
        await Task.Delay(1_100); // rating süresi > 0
        await host.PostAsJsonAsync($"/api/v1/floor/ratings/{ratingId}/finish", new { averageBet = 100_000m, reasonCode = "RATE" });

        var summary = await WaitForPoints(host, player.PlayerId);
        Assert.True(summary.Balance > 0);

        var comp = await host.PostAsJsonAsync("/api/v1/loyalty/comps", new { playerId = player.PlayerId, points = 1L, text = "Kahve", reasonCode = "COMP" });
        var compId = (await comp.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        var selfApprove = await host.PostAsJsonAsync($"/api/v1/loyalty/comps/{compId}/decision", new { approve = true, reasonCode = "COMP" });

        var manager = UserId.New();
        await api.AddUser(manager);
        await TestRoles.GrantTo(host, manager, "loyalty.comp:approve");
        var approve = await api.ClientFor(manager).PostAsJsonAsync($"/api/v1/loyalty/comps/{compId}/decision", new { approve = true, reasonCode = "COMP" });
        var after = await host.GetFromJsonAsync<LoyaltySummary>($"/api/v1/loyalty/players/{player.PlayerId}");

        Assert.Equal(HttpStatusCode.Created, comp.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, selfApprove.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);
        Assert.Equal(summary.Balance - 1, after!.Balance);
    }

    private static async Task<LoyaltySummary> WaitForPoints(HttpClient client, Guid playerId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            var summary = (await client.GetFromJsonAsync<LoyaltySummary>($"/api/v1/loyalty/players/{playerId}"))!;
            if (summary.Balance > 0 || DateTime.UtcNow > deadline)
            {
                return summary;
            }

            await Task.Delay(200);
        }
    }

    private sealed record IdResponse(Guid Id);
}
