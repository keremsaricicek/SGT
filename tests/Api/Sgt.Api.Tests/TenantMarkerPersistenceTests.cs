using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Markers;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Api.Tests;

/// <summary>configuration.tenant_markers: append-only tetikleyici ve eşzamanlı ekleme (Spec 012).</summary>
public sealed class TenantMarkerPersistenceTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    private static AuditContext Audit() => new(ApiFactory.SuperAdmin, "TEST", ReasonCode.Of("TEST"), Guid.NewGuid());

    [Theory]
    [InlineData("UPDATE configuration.tenant_markers SET terminal = 'X' WHERE tenant_id = @t")]
    [InlineData("DELETE FROM configuration.tenant_markers WHERE tenant_id = @t")]
    public async Task Marker_rows_cannot_be_updated_or_deleted(string sql)
    {
        var tenant = new TenantId(Guid.NewGuid());
        Assert.True((await Set(tenant, "APPEND_ONLY")).IsSuccess);

        await using var connection = new NpgsqlConnection(api.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("t", tenant.Value);

        var error = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Contains("append-only", error.MessageText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Duplicate_insert_at_commit_raises_conflict_not_database_error()
    {
        var tenant = new TenantId(Guid.NewGuid());
        Assert.True((await Set(tenant, "RACE")).IsSuccess);

        await using var scope = api.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ITenantMarkerStore>().Add(TenantMarker.Set(tenant, "RACE", Audit(), DateTimeOffset.UnixEpoch).Value);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => scope.ServiceProvider.GetRequiredService<IConfigurationUnitOfWork>().Commit(default));
    }

    [Fact]
    public async Task Concurrent_requests_set_the_marker_exactly_once()
    {
        var tenant = new TenantId(Guid.NewGuid());

        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() => Set(tenant, "CONCURRENT"))));

        Assert.Single(results, r => r.IsSuccess);
        Assert.All(results.Where(r => r.IsFailure), r => Assert.Equal(ConfigurationErrors.MarkerAlreadySet, r.Error));
    }

    private async Task<Result> Set(TenantId tenant, string code)
    {
        await using var scope = api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICommandHandler<SetTenantMarkerCommand>>()
            .Handle(new SetTenantMarkerCommand(tenant, code, Audit()), default);
    }
}
