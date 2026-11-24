using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Sgt.BuildingBlocks.Infrastructure.Locking;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed class AdvisoryLockTests(MessagingFixture fx) : IClassFixture<MessagingFixture>
{
    [Fact]
    public async Task Lock_is_exclusive_per_name_until_released()
    {
        var locks = new PostgresAdvisoryLock(fx.ConnectionString, NullLogger<PostgresAdvisoryLock>.Instance);
        var name = $"test:{Guid.NewGuid()}";

        var first = await locks.TryAcquire(name, default);
        Assert.NotNull(first);
        Assert.Null(await locks.TryAcquire(name, default));

        var other = await locks.TryAcquire(name + ":other", default);
        Assert.NotNull(other);
        await other.DisposeAsync();

        await first.DisposeAsync();
        var again = await locks.TryAcquire(name, default);
        Assert.NotNull(again);
        await again.DisposeAsync();
    }

    [Fact]
    public async Task Releasing_a_lock_whose_connection_died_does_not_throw()
    {
        // Bırakma hatası, kilidi tutan işlemin asıl hatasını örtmemeli; bağlantı kapanınca PostgreSQL kilidi zaten çözer.
        var locks = new PostgresAdvisoryLock(fx.ConnectionString, NullLogger<PostgresAdvisoryLock>.Instance);
        var held = await locks.TryAcquire($"test:{Guid.NewGuid()}", default);
        Assert.NotNull(held);

        await using (var admin = new NpgsqlConnection(fx.ConnectionString))
        {
            await admin.OpenAsync();
            await using var kill = new NpgsqlCommand(
                "SELECT pg_terminate_backend(l.pid) FROM pg_locks l JOIN pg_database d ON d.oid = l.database " +
                "WHERE l.locktype = 'advisory' AND d.datname = current_database() AND l.pid <> pg_backend_pid()", admin);
            await kill.ExecuteNonQueryAsync();
        }

        await held.DisposeAsync();
    }
}
