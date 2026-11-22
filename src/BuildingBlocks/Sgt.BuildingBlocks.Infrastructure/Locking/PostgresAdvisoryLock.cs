using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Sgt.BuildingBlocks.Infrastructure.Locking;

/// <summary>
/// PostgreSQL oturum düzeyi advisory lock. Kilit, kendi bağlantısı açık kaldıkça tutulur; bırakılınca veya süreç
/// ölünce PostgreSQL çözer. Ad, SHA-256 ile 64 bit anahtara çevrilir.
/// </summary>
public sealed class PostgresAdvisoryLock(string connectionString) : IAdvisoryLock
{
    public async Task<IAsyncDisposable?> TryAcquire(string name, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var key = Key(name);
        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(ct);
            await using var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@key)", connection);
            command.Parameters.AddWithValue("key", key);
            if (await command.ExecuteScalarAsync(ct) is true)
            {
                return new Held(connection, key);
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }

        await connection.DisposeAsync();
        return null;
    }

    private static long Key(string name) => BitConverter.ToInt64(SHA256.HashData(Encoding.UTF8.GetBytes(name)), 0);

    private sealed class Held(NpgsqlConnection connection, long key) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await using (connection)
            {
                await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", connection);
                command.Parameters.AddWithValue("key", key);
                await command.ExecuteScalarAsync();
            }
        }
    }
}

public static class AdvisoryLockServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresAdvisoryLock(this IServiceCollection services, string connectionString) =>
        services.AddSingleton<IAdvisoryLock>(new PostgresAdvisoryLock(connectionString));
}
