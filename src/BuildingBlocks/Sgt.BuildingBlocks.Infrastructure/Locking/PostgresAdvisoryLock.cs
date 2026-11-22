using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Sgt.BuildingBlocks.Infrastructure.Locking;

/// <summary>
/// PostgreSQL oturum düzeyi advisory lock. Kilit, kendi bağlantısı açık kaldıkça tutulur; bırakılınca veya süreç
/// ölünce PostgreSQL çözer. Ad, SHA-256 ile 64 bit anahtara çevrilir.
/// </summary>
public sealed partial class PostgresAdvisoryLock(string connectionString, ILogger<PostgresAdvisoryLock> logger) : IAdvisoryLock
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
                return new Held(connection, key, name, logger);
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Advisory lock bırakılamadı ({Name}); bağlantı kapandığı için PostgreSQL kilidi çözer.")]
    private static partial void LogReleaseFailed(ILogger logger, string name, Exception exception);

    /// <summary>Bırakma hatası fırlatılmaz (loglanır): kilidi tutan işlemin asıl hatasını örtmemeli.</summary>
    private sealed class Held(NpgsqlConnection connection, long key, string name, ILogger logger) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", connection);
                command.Parameters.AddWithValue("key", key);
                await command.ExecuteScalarAsync();
            }
            catch (Exception e) when (e is NpgsqlException or InvalidOperationException)
            {
                LogReleaseFailed(logger, name, e);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}

public static class AdvisoryLockServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresAdvisoryLock(this IServiceCollection services, string connectionString) =>
        services.AddSingleton<IAdvisoryLock>(sp => new PostgresAdvisoryLock(connectionString, sp.GetRequiredService<ILogger<PostgresAdvisoryLock>>()));
}
