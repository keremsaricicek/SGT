using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Contracts;

namespace Sgt.Api.Tests;

public sealed class ExchangeRateApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Authorized_user_sets_rate_everyone_reads_it_and_history_is_append_only()
    {
        var staff = UserId.New();
        await api.AddUser(staff);
        var admin = api.ClientFor(ApiFactory.SuperAdmin);

        var set = await admin.PostAsJsonAsync("/api/configuration/exchange-rates", new { currency = "EUR", buy = 36.5m, sell = 37.25m, reasonCode = "DAILY_RATE" });
        var byStaff = await api.ClientFor(staff).PostAsJsonAsync("/api/configuration/exchange-rates", new { currency = "EUR", buy = 1m, sell = 2m, reasonCode = "X" });
        var current = await api.ClientFor(staff).GetFromJsonAsync<List<ExchangeRateDto>>("/api/configuration/exchange-rates/current");

        Assert.Equal(HttpStatusCode.NoContent, set.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, byStaff.StatusCode);
        var eur = Assert.Single(current!);
        Assert.Equal(37.25m, eur.Sell);

        await using var connection = new NpgsqlConnection(api.ConnectionString);
        await connection.OpenAsync();
        await using var update = new NpgsqlCommand("UPDATE configuration.exchange_rates SET sell = 1", connection);
        await Assert.ThrowsAsync<PostgresException>(() => update.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task Invalid_rate_is_rejected()
    {
        var response = await api.ClientFor(ApiFactory.SuperAdmin)
            .PostAsJsonAsync("/api/configuration/exchange-rates", new { currency = "EUR", buy = 40m, sell = 39m, reasonCode = "DAILY_RATE" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Configuration.BuyAboveSell", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }
}
