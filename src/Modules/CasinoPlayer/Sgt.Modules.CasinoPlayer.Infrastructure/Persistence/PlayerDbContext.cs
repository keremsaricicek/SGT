using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Modules.CasinoPlayer.Infrastructure.Persistence;

/// <summary>CasinoPlayer modülünün şeması. Belge numarası ve telefon şifrelidir (ADR-0005).</summary>
public sealed class PlayerDbContext(DbContextOptions<PlayerDbContext> options) : DbContext(options)
{
    public const string Schema = "casino_player";

    internal DbSet<PlayerRow> Players => Set<PlayerRow>();

    internal DbSet<PlayerCardRow> Cards => Set<PlayerCardRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddOutboxAndInbox();

        modelBuilder.Entity<PlayerRow>(e =>
        {
            e.ToTable("players");
            e.HasKey(p => p.Id);
            e.HasIndex(p => new { p.TenantId, p.Number }).IsUnique();
            e.HasIndex(p => new { p.TenantId, p.DocumentType, p.DocumentIndex }).IsUnique();
            e.HasIndex(p => new { p.TenantId, p.LastName });
            e.HasIndex(p => new { p.TenantId, p.FirstName });
            e.Property(p => p.Number).HasMaxLength(10);
            e.Property(p => p.FirstName).HasMaxLength(100);
            e.Property(p => p.LastName).HasMaxLength(100);
            e.Property(p => p.Nationality).HasMaxLength(2);
            e.Property(p => p.DocumentType).HasMaxLength(20);
            e.Property(p => p.DocumentIndex).HasMaxLength(64);
            e.Property(p => p.Version).IsRowVersion();
            e.HasMany(p => p.Cards).WithOne().HasForeignKey(c => c.PlayerId);
            e.HasMany(p => p.Restrictions).WithOne().HasForeignKey(r => r.PlayerId);
        });

        modelBuilder.Entity<PlayerCardRow>(e =>
        {
            e.ToTable("player_cards");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedNever(); // yeni satır gezinme ile eklenir; anahtar uygulamadan gelir
            e.HasIndex(c => new { c.TenantId, c.Number }).IsUnique();
            e.Property(c => c.Number).HasMaxLength(20);
        });

        modelBuilder.Entity<PlayerRestrictionRow>(e =>
        {
            e.ToTable("player_restrictions");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).ValueGeneratedNever();
            e.Property(r => r.Type).HasMaxLength(20);
            e.Property(r => r.Note).HasMaxLength(500);
        });

        modelBuilder.Entity<PlayerSequenceRow>(e =>
        {
            e.ToTable("player_sequences");
            e.HasKey(s => s.TenantId);
        });
    }
}

internal sealed class PlayerRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Number { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public string Nationality { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    /// <summary>Şifreli belge numarası.</summary>
    public string DocumentCipher { get; set; } = string.Empty;

    /// <summary>Belge numarasının kör indeksi; arama ve tekillik için.</summary>
    public string DocumentIndex { get; set; } = string.Empty;

    public string? PhoneCipher { get; set; }

    public uint Version { get; set; }

    public List<PlayerCardRow> Cards { get; set; } = [];

    public List<PlayerRestrictionRow> Restrictions { get; set; } = [];
}

internal sealed class PlayerCardRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PlayerId { get; set; }

    public string Number { get; set; } = string.Empty;

    public DateTimeOffset IssuedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}

internal sealed class PlayerRestrictionRow
{
    public Guid Id { get; set; }

    public Guid PlayerId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTimeOffset? Until { get; set; }

    public Guid AddedBy { get; set; }

    public DateTimeOffset AddedAt { get; set; }

    public Guid? RemovedBy { get; set; }

    public DateTimeOffset? RemovedAt { get; set; }
}

internal sealed class PlayerSequenceRow
{
    public Guid TenantId { get; set; }

    public long LastValue { get; set; }
}
