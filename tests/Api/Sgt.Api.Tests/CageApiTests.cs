using System.Net;
using System.Net.Http.Json;
using Npgsql;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.Finance.Application.Cage;

namespace Sgt.Api.Tests;

public sealed class CageApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Cashier_shift_end_to_end_with_idempotency_exchange_and_variance()
    {
        var cashier = api.ClientFor(ApiFactory.SuperAdmin);
        await Grant(cashier,
            "player.profile:create", "cage.shift:open", "cage.shift:close", "cage.summary:view", "cage.chips:sell", "cage.chips:redeem", "cage.exchange:exchange");
        var player = (await (await cashier.PostAsJsonAsync("/api/v1/players", new
        {
            firstName = "Kasa", lastName = "Deneme", birthDate = "1985-01-01", nationality = "TR", documentType = "NationalId",
            documentNumber = $"K{Guid.NewGuid():N}"[..12], reasonCode = "NEW_PLAYER",
        })).Content.ReadFromJsonAsync<PlayerRegistered>())!;
        await cashier.PostAsJsonAsync("/api/v1/configuration/exchange-rates", new { currency = "EUR", buy = 36m, sell = 37m, reasonCode = "DAILY_RATE" });

        var opened = await cashier.PostAsJsonAsync("/api/v1/cage/shifts", new
        {
            opening = new[]
            {
                new { instrument = "Cash", currency = "TRY", amount = 10_000m },
                new { instrument = "Chips", currency = "TRY", amount = 20_000m },
            },
            reasonCode = "SHIFT_START",
        });
        var shiftId = (await opened.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        var key = Guid.NewGuid();
        var sale = new { transactionId = key, playerId = player.PlayerId, amount = 1_500m, currency = "TRY", reasonCode = "CHIP_SALE" };
        var first = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/chip-sales", sale);
        var repeat = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/chip-sales", sale);
        var exchange = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/exchanges/foreign-to-base",
            new { transactionId = Guid.NewGuid(), playerId = player.PlayerId, amount = 100m, currency = "EUR", reasonCode = "FX" });

        var summary = await cashier.GetFromJsonAsync<ShiftSummary>($"/api/v1/cage/shifts/{shiftId}");

        Assert.Equal(HttpStatusCode.Created, opened.StatusCode);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeat.StatusCode);
        Assert.Equal(HttpStatusCode.OK, exchange.StatusCode);
        Assert.Equal(2, summary!.Transactions.Count);
        Assert.Contains(summary.Balances, b => b.Instrument == "Cash" && b.Currency == "TRY" && b.Expected == 10_000m + 1_500m - 3_600m);
        Assert.Contains(summary.Balances, b => b.Instrument == "Cash" && b.Currency == "EUR" && b.Expected == 100m);
        Assert.Contains(summary.Balances, b => b.Instrument == "Chips" && b.Currency == "TRY" && b.Expected == 18_500m);

        var counted = new[]
        {
            new { instrument = "Cash", currency = "TRY", amount = 8_000m },
            new { instrument = "Chips", currency = "TRY", amount = 18_500m },
            new { instrument = "Cash", currency = "EUR", amount = 100m },
        };
        var withoutNote = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/close", new { counted, note = (string?)null, reasonCode = "SHIFT_END" });
        var withNote = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/close", new { counted, note = "Sayımda 100 TL fazla", reasonCode = "SHIFT_END" });
        var afterClose = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/chip-sales",
            new { transactionId = Guid.NewGuid(), playerId = player.PlayerId, amount = 10m, currency = "TRY", reasonCode = "CHIP_SALE" });

        Assert.Equal(HttpStatusCode.BadRequest, withoutNote.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, withNote.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, afterClose.StatusCode);
    }

    [Fact]
    public async Task Ledger_is_append_only_and_must_balance()
    {
        await using var connection = new NpgsqlConnection(api.ConnectionString);
        await connection.OpenAsync();

        var balanced = await InsertTransaction(connection, entries: [("Cash", 10m), ("Chips", -10m)]);
        Assert.Null(balanced);

        await using var update = new NpgsqlCommand("UPDATE finance.ledger_entries SET amount = 0", connection);
        await Assert.ThrowsAsync<PostgresException>(() => update.ExecuteNonQueryAsync());

        var unbalanced = await InsertTransaction(connection, entries: [("Cash", 10m)]);
        Assert.NotNull(unbalanced);
    }

    // Bir vardiya + işlem + defter satırlarını tek transaction'da yazar; commit hatasını döner.
    private static async Task<PostgresException?> InsertTransaction(NpgsqlConnection connection, (string Instrument, decimal Amount)[] entries)
    {
        await using var tx = await connection.BeginTransactionAsync();
        var shiftId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var sql = $"""
            INSERT INTO finance.cashier_shifts (id, tenant_id, cashier_id, terminal, opened_at) VALUES ('{shiftId}', gen_random_uuid(), gen_random_uuid(), 'T-{shiftId:N}', now());
            INSERT INTO finance.cage_transactions (id, tenant_id, shift_id, type, player_id, amount, currency, actor_id, terminal, reason_code, correlation_id, occurred_at)
              VALUES ('{transactionId}', gen_random_uuid(), '{shiftId}', 'ChipSale', gen_random_uuid(), 10, 'TRY', gen_random_uuid(), 'T', 'R', gen_random_uuid(), now());
            """ + string.Concat(entries.Select(e =>
            $"INSERT INTO finance.ledger_entries (id, transaction_id, account, instrument, currency, amount) VALUES (gen_random_uuid(), '{transactionId}', 'Drawer', '{e.Instrument}', 'TRY', {e.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture)});"));
        await using var insert = new NpgsqlCommand(sql, connection, tx);
        await insert.ExecuteNonQueryAsync();
        try
        {
            await tx.CommitAsync();
            return null;
        }
        catch (PostgresException e)
        {
            return e;
        }
    }

    private static async Task Grant(HttpClient admin, params string[] permissions)
    {
        var role = await admin.PostAsJsonAsync("/api/v1/identity/roles", new { name = $"Kasa {Guid.NewGuid():N}", reasonCode = "TEST" });
        var roleId = (await role.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        foreach (var screen in permissions.Select(p => p.Split(':')[0]).Distinct())
        {
            await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = $"{screen}:view", reasonCode = "TEST" });
        }

        foreach (var p in permissions)
        {
            await admin.PostAsJsonAsync($"/api/v1/identity/roles/{roleId}/permissions", new { permission = p, reasonCode = "TEST" });
        }

        await admin.PostAsJsonAsync($"/api/v1/identity/users/{ApiFactory.SuperAdmin.Value}/roles", new { roleId, reasonCode = "TEST" });
    }

    private sealed record IdResponse(Guid Id);
}

public sealed class CageAdjustmentApiTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Adjustment_requires_second_person_and_reverses_transaction()
    {
        var cashier = api.ClientFor(ApiFactory.SuperAdmin);
        await TestRoles.Grant(cashier, "player.profile:create", "cage.shift:open", "cage.summary:view", "cage.chips:sell", "cage.adjustment:request", "cage.adjustment:approve");
        var player = (await (await cashier.PostAsJsonAsync("/api/v1/players", new
        {
            firstName = "Düzelt", lastName = "Me", birthDate = "1985-01-01", nationality = "TR", documentType = "NationalId",
            documentNumber = $"D{Guid.NewGuid():N}"[..12], reasonCode = "NEW_PLAYER",
        })).Content.ReadFromJsonAsync<Sgt.Modules.CasinoPlayer.Application.Profile.PlayerRegistered>())!;
        var shiftId = (await (await cashier.PostAsJsonAsync("/api/v1/cage/shifts", new
        {
            opening = new[] { new { instrument = "Cash", currency = "TRY", amount = 1_000m }, new { instrument = "Chips", currency = "TRY", amount = 5_000m } },
            reasonCode = "SHIFT_START",
        })).Content.ReadFromJsonAsync<IdResponse>())!.Id;
        var transactionId = Guid.NewGuid();
        await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/chip-sales",
            new { transactionId, playerId = player.PlayerId, amount = 700m, currency = "TRY", reasonCode = "CHIP_SALE" });

        var request = await cashier.PostAsJsonAsync($"/api/v1/cage/shifts/{shiftId}/adjustments", new { transactionId, note = "Yanlış tutar girildi", reasonCode = "CORRECTION" });
        var requestId = (await request.Content.ReadFromJsonAsync<IdResponse>())!.Id;
        var self = await cashier.PostAsJsonAsync($"/api/v1/cage/adjustments/{requestId}/decision", new { approve = true, reasonCode = "CORRECTION" });

        var supervisor = Sgt.BuildingBlocks.UserId.New();
        await api.AddUser(supervisor);
        await TestRoles.GrantTo(cashier, supervisor, "cage.adjustment:approve");
        var approved = await api.ClientFor(supervisor).PostAsJsonAsync($"/api/v1/cage/adjustments/{requestId}/decision", new { approve = true, reasonCode = "CORRECTION" });
        var summary = await cashier.GetFromJsonAsync<ShiftSummary>($"/api/v1/cage/shifts/{shiftId}");

        Assert.Equal(HttpStatusCode.Created, request.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, self.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, approved.StatusCode);
        Assert.Contains(summary!.Transactions, t => t.Type == "Reversal");
        Assert.Contains(summary.Balances, b => b.Instrument == "Cash" && b.Currency == "TRY" && b.Expected == 1_000m);
    }

    private sealed record IdResponse(Guid Id);
}
