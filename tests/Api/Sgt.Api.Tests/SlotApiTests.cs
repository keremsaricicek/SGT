using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Sgt.Modules.GamingFloor.Application.Slots;

namespace Sgt.Api.Tests;

public sealed class SlotApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Define_enter_import_and_view_slot_floor()
    {
        var slot = api.ClientFor(ApiFactory.SuperAdmin);
        await TestRoles.Grant(slot, "slot.machines_setup:define", "slot.manual_entry:enter", "slot.manual_entry:import", "slot.floor:view", "slot.machine:view");
        var asset = $"A-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

        var defined = await slot.PostAsJsonAsync("/api/floor/slots", new
        {
            assetNumber = asset, area = "Salon 1", manufacturer = "IGT", model = "S3000", denomination = 0.05m, currency = "TRY", reasonCode = "SETUP",
        });
        var machineId = (await defined.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        var now = DateTimeOffset.UtcNow;

        var manual = await slot.PostAsJsonAsync("/api/floor/slots/readings", new
        {
            assetNumber = asset, readAt = now.AddMinutes(-30), coinIn = 1_000m, coinOut = 700m, jackpot = 0m, gamesPlayed = 90L, isReset = false,
            reference = "m-1", reasonCode = "METERS",
        });
        var csv = $"asset_number,read_at,coin_in,coin_out,jackpot,games_played\n{asset},{now.AddMinutes(-10):O},1500,900,100,150\n{asset},{now.AddMinutes(-5):O},1,1,0,1\n";
        var import = await slot.PostAsJsonAsync("/api/floor/slots/readings/import", new { csv, reasonCode = "IMPORT" });
        var reimport = await slot.PostAsJsonAsync("/api/floor/slots/readings/import", new { csv, reasonCode = "IMPORT" });
        var report = await import.Content.ReadFromJsonAsync<ImportReport>();
        var detail = await slot.GetFromJsonAsync<SlotMachineView>($"/api/floor/slots/{machineId}");
        var floor = await slot.GetFromJsonAsync<List<SlotFloorRow>>("/api/floor/slots");

        Assert.Equal(HttpStatusCode.Created, defined.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, manual.StatusCode);
        Assert.Equal(1, report!.Imported);
        Assert.Equal([3], report.Errors.Select(e => e.Line));
        Assert.Equal(HttpStatusCode.OK, reimport.StatusCode);
        Assert.Equal(2, detail!.Readings.Count);
        Assert.Equal(200m, detail.Readings[0].DeltaWin); // 500 − 200 − 100
        Assert.Contains(floor!, f => f.AssetNumber == asset);

        await using var connection = new NpgsqlConnection(api.ConnectionString);
        await connection.OpenAsync();
        await using var delete = new NpgsqlCommand("DELETE FROM gaming_floor.meter_readings", connection);
        await Assert.ThrowsAsync<PostgresException>(() => delete.ExecuteNonQueryAsync());
    }

    private sealed record IdResponse(Guid Id);
}
