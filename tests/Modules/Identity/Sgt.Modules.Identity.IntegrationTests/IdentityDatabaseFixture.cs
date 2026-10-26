using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Infrastructure;
using Sgt.Modules.Identity.Infrastructure.Persistence;

namespace Sgt.Modules.Identity.IntegrationTests;

/// <summary>
/// Her test sınıfı için boş bir PostgreSQL veritabanı oluşturur ve migration'ları uygular.
/// Bağlantı: SGT_TEST_DB ortam değişkeni, yoksa yerel geliştirme veritabanı.
/// </summary>
public sealed class IdentityDatabaseFixture : IAsyncLifetime
{
    private const string LocalServer = "Host=localhost;Username=sgt;Password=sgt_dev_only";

    private readonly string _server = Environment.GetEnvironmentVariable("SGT_TEST_DB") ?? LocalServer;
    private readonly string _database = $"sgt_identity_test_{Guid.NewGuid():N}";

    public ServiceProvider Services { get; private set; } = null!;

    public string ConnectionString => new NpgsqlConnectionStringBuilder(_server) { Database = _database }.ConnectionString;

    public async Task InitializeAsync()
    {
        Services = new ServiceCollection()
            .AddSingleton<IClock, SystemClock>()
            .AddIdentityModule(ConnectionString)
            .BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.MigrateIdentityDatabase(default);
    }

    public async Task DisposeAsync()
    {
        await using (var scope = Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureDeletedAsync();
        }

        await Services.DisposeAsync();
    }
}
