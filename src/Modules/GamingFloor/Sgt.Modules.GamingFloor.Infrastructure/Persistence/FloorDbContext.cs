using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Modules.GamingFloor.Infrastructure.Persistence;

/// <summary>GamingFloor şeması. rating_corrections append-only'dir (Spec 007 KK6).</summary>
public sealed class FloorDbContext(DbContextOptions<FloorDbContext> options) : DbContext(options)
{
    public const string Schema = "gaming_floor";

    internal DbSet<TableRow> Tables => Set<TableRow>();

    internal DbSet<RatingRow> Ratings => Set<RatingRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddOutboxAndInbox();

        modelBuilder.Entity<TableRow>(e =>
        {
            e.ToTable("gaming_tables");
            e.HasKey(t => t.Id);
            e.HasIndex(t => new { t.TenantId, t.Code }).IsUnique();
            e.Property(t => t.Code).HasMaxLength(20);
            e.Property(t => t.GameType).HasMaxLength(20);
            e.Property(t => t.Currency).HasMaxLength(3);
            e.Property(t => t.MinBet).HasPrecision(18, 2);
            e.Property(t => t.MaxBet).HasPrecision(18, 2);
            e.Property(t => t.HouseEdgePercent).HasPrecision(5, 2);
            e.Property(t => t.Version).IsRowVersion();
            e.HasMany(t => t.Sessions).WithOne().HasForeignKey(s => s.TableId);
        });

        modelBuilder.Entity<TableSessionRow>(e =>
        {
            e.ToTable("table_sessions");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).ValueGeneratedNever();
            e.HasIndex(s => s.TableId).IsUnique().HasFilter("closed_at IS NULL");
        });

        modelBuilder.Entity<RatingRow>(e =>
        {
            e.ToTable("ratings");
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.TenantId, r.PlayerId }).IsUnique().HasFilter("finished_at IS NULL");
            e.HasIndex(r => new { r.TenantId, r.TableId }).HasFilter("finished_at IS NULL");
            e.Property(r => r.TableCode).HasMaxLength(20);
            e.Property(r => r.Currency).HasMaxLength(3);
            e.Property(r => r.HouseEdgePercent).HasPrecision(5, 2);
            e.Property(r => r.AverageBet).HasPrecision(18, 2);
            e.Property(r => r.PlayerResult).HasPrecision(18, 2);
            e.Property(r => r.TheoreticalWin).HasPrecision(18, 2);
            e.Property(r => r.Version).IsRowVersion();
            e.HasMany(r => r.Corrections).WithOne().HasForeignKey(c => c.RatingId);
        });

        modelBuilder.Entity<RatingCorrectionRow>(e =>
        {
            e.ToTable("rating_corrections");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).ValueGeneratedNever();
            e.Property(c => c.PreviousAverageBet).HasPrecision(18, 2);
            e.Property(c => c.PreviousPlayerResult).HasPrecision(18, 2);
            e.Property(c => c.AverageBet).HasPrecision(18, 2);
            e.Property(c => c.PlayerResult).HasPrecision(18, 2);
            e.Property(c => c.Terminal).HasMaxLength(100);
            e.Property(c => c.ReasonCode).HasMaxLength(50);
        });
    }
}

internal sealed class TableRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string GameType { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal MinBet { get; set; }

    public decimal MaxBet { get; set; }

    public decimal HouseEdgePercent { get; set; }

    public int HandsPerHour { get; set; }

    public uint Version { get; set; }

    public List<TableSessionRow> Sessions { get; set; } = [];
}

internal sealed class TableSessionRow
{
    public Guid Id { get; set; }

    public Guid TableId { get; set; }

    public DateTimeOffset OpenedAt { get; set; }

    public Guid OpenedBy { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public Guid? ClosedBy { get; set; }
}

internal sealed class RatingRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PlayerId { get; set; }

    public Guid TableId { get; set; }

    public string TableCode { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal HouseEdgePercent { get; set; }

    public int HandsPerHour { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public Guid StartedBy { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public decimal? AverageBet { get; set; }

    public decimal? PlayerResult { get; set; }

    public decimal? TheoreticalWin { get; set; }

    public uint Version { get; set; }

    public List<RatingCorrectionRow> Corrections { get; set; } = [];
}

internal sealed class RatingCorrectionRow
{
    public Guid Id { get; set; }

    public Guid RatingId { get; set; }

    public decimal PreviousAverageBet { get; set; }

    public decimal? PreviousPlayerResult { get; set; }

    public decimal AverageBet { get; set; }

    public decimal? PlayerResult { get; set; }

    public Guid ActorId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public DateTimeOffset At { get; set; }
}
