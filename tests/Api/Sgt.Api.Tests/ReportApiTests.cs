using System.Net.Http.Json;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.Reporting.Application.Daily;

namespace Sgt.Api.Tests;

public sealed class ReportApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Daily_summary_reflects_cage_activity_net_of_reversals()
    {
        var manager = api.ClientFor(ApiFactory.SuperAdmin);
        await TestRoles.Grant(manager, "player.profile:create", "cage.shift:open", "cage.chips:sell", "report.daily:view");
        var player = (await (await manager.PostAsJsonAsync("/api/v1/players", new
        {
            firstName = "Rapor", lastName = "Test", birthDate = "1990-01-01", nationality = "TR", documentType = "NationalId",
            documentNumber = $"R{Guid.NewGuid():N}"[..12], reasonCode = "NEW_PLAYER",
        })).Content.ReadFromJsonAsync<PlayerRegistered>())!;
        var shiftId = (await (await manager.PostAsJsonAsync("/api/v1/cage/shifts", new
        {
            opening = new[] { new { instrument = "Chips", currency = "TRY", amount = 10_000m } }, reasonCode = "SHIFT_START",
        })).Content.ReadFromJsonAsync<IdResponse>())!.Id;
        await manager.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/chip-sales",
            new { transactionId = Guid.NewGuid(), playerId = player.PlayerId, amount = 2_500m, currency = "TRY", reasonCode = "CHIP_SALE" });

        var report = await manager.GetFromJsonAsync<DailySummary>("/api/v1/reports/daily");

        Assert.True(report!.Cage.ShiftsOpen >= 1);
        Assert.Contains(report.Cage.Currencies, c => c.Currency == "TRY" && c.ChipSales >= 2_500m);
    }

    private sealed record IdResponse(Guid Id);
}
