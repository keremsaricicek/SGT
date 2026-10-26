using Microsoft.EntityFrameworkCore;
using Sgt.Modules.Identity.Infrastructure.Licensing;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

/// <summary>Identity modülünün kendi veritabanı şeması. Başka modül bu tablolara erişmez (K2).</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public const string Schema = "identity";

    internal DbSet<RoleRow> Roles => Set<RoleRow>();

    internal DbSet<UserAccessRow> Users => Set<UserAccessRow>();

    internal DbSet<PermissionChangeRow> PermissionChanges => Set<PermissionChangeRow>();

    internal DbSet<TenantLicenseRow> TenantLicenses => Set<TenantLicenseRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<RoleRow>(e =>
        {
            e.ToTable("roles");
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.TenantId);
            e.Property(r => r.Name).HasMaxLength(100);
            e.Property(r => r.Version).IsRowVersion();
        });

        modelBuilder.Entity<UserAccessRow>(e =>
        {
            e.ToTable("user_access");
            e.HasKey(u => new { u.TenantId, u.UserId });
            e.Property(u => u.Version).IsRowVersion();
        });

        modelBuilder.Entity<PermissionChangeRow>(e =>
        {
            e.ToTable("permission_changes");
            e.HasKey(c => c.Id);
            e.HasIndex(c => new { c.TenantId, c.OccurredAt });
            e.Property(c => c.Target).HasMaxLength(20);
            e.Property(c => c.Kind).HasMaxLength(40);
            e.Property(c => c.Item).HasMaxLength(200);
            e.Property(c => c.Terminal).HasMaxLength(100);
            e.Property(c => c.ReasonCode).HasMaxLength(50);
        });

        modelBuilder.Entity<TenantLicenseRow>(e =>
        {
            e.ToTable("tenant_licenses");
            e.HasKey(l => new { l.TenantId, l.Product });
            e.Property(l => l.Product).HasMaxLength(20);
        });
    }
}
