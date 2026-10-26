using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Modules.Loyalty.Infrastructure.Persistence;

/// <summary>Loyalty şeması. points_entries append-only'dir (Spec 008 KK3).</summary>
public sealed class LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options) : DbContext(options)
{
    public const string Schema = "loyalty";

    internal DbSet<AccountRow> Accounts => Set<AccountRow>();

    internal DbSet<CompRequestRow> Comps => Set<CompRequestRow>();

    internal DbSet<AdjustmentRequestRow> Adjustments => Set<AdjustmentRequestRow>();

    internal DbSet<EarningRuleRow> Rules => Set<EarningRuleRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddOutboxAndInbox();

        modelBuilder.Entity<AccountRow>(e =>
        {
            e.ToTable("accounts");
            e.HasKey(a => new { a.TenantId, a.PlayerId });
            e.Property(a => a.Version).IsRowVersion();
            e.HasMany(a => a.Entries).WithOne().HasForeignKey(x => new { x.TenantId, x.PlayerId });
        });

        modelBuilder.Entity<PointsEntryRow>(e =>
        {
            e.ToTable("points_entries");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.TenantId, x.PlayerId, x.SourceId });
            e.Property(x => x.Kind).HasMaxLength(20);
            e.Property(x => x.Terminal).HasMaxLength(100);
            e.Property(x => x.ReasonCode).HasMaxLength(50);
        });

        foreach (var entity in new[] { typeof(CompRequestRow), typeof(AdjustmentRequestRow) })
        {
            modelBuilder.Entity(entity, e =>
            {
                e.ToTable(entity == typeof(CompRequestRow) ? "comp_requests" : "adjustment_requests");
                e.HasKey(nameof(RequestRow.Id));
                e.HasIndex(nameof(RequestRow.TenantId), nameof(RequestRow.PlayerId));
                e.Property(nameof(RequestRow.Text)).HasMaxLength(500);
                e.Property(nameof(RequestRow.Status)).HasMaxLength(20);
                e.Property<uint>(nameof(RequestRow.Version)).IsRowVersion();
            });
        }

        modelBuilder.Entity<EarningRuleRow>(e =>
        {
            e.ToTable("earning_rules");
            e.HasKey(r => r.TenantId);
            e.Property(r => r.TryPerPoint).HasPrecision(18, 2);
        });
    }
}

internal sealed class AccountRow
{
    public Guid TenantId { get; set; }

    public Guid PlayerId { get; set; }

    /// <summary>Her hareket bu sayıyı değiştirir; böylece eşzamanlı harcamalar xmin ile çakışır.</summary>
    public int EntryCount { get; set; }

    public uint Version { get; set; }

    public List<PointsEntryRow> Entries { get; set; } = [];
}

internal sealed class PointsEntryRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PlayerId { get; set; }

    public string Kind { get; set; } = string.Empty;

    public long Points { get; set; }

    public Guid SourceId { get; set; }

    public Guid ActorId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public DateTimeOffset At { get; set; }
}

internal abstract class RequestRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PlayerId { get; set; }

    public long Points { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTimeOffset RequestedAt { get; set; }

    public Guid RequestedBy { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? DecidedBy { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public uint Version { get; set; }
}

internal sealed class CompRequestRow : RequestRow;

internal sealed class AdjustmentRequestRow : RequestRow;

internal sealed class EarningRuleRow
{
    public Guid TenantId { get; set; }

    public decimal TryPerPoint { get; set; }
}
