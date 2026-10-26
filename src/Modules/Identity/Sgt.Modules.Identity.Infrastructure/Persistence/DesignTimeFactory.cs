using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

/// <summary>Yalnızca "dotnet ef migrations" komutu içindir; bağlantı kurmaz.</summary>
internal sealed class DesignTimeFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", IdentityDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}
