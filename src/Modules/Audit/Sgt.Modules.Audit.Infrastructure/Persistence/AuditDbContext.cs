using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Modules.Audit.Infrastructure.Persistence;

/// <summary>Audit modülünün şeması. audit_entries append-only'dir (tetikleyici, KK7).</summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public const string Schema = "audit";

    internal DbSet<AuditEntryRow> Entries => Set<AuditEntryRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddOutboxAndInbox();

        modelBuilder.Entity<AuditEntryRow>(e =>
        {
            e.ToTable("audit_entries");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.OccurredAt });
            e.HasIndex(x => new { x.TenantId, x.CorrelationId });
            e.HasIndex(x => new { x.TenantId, x.ActorId, x.OccurredAt });
            e.Property(x => x.EventType).HasMaxLength(300);
            e.Property(x => x.Summary).HasMaxLength(1000);
            e.Property(x => x.Terminal).HasMaxLength(100);
            e.Property(x => x.ReasonCode).HasMaxLength(50);
        });
    }
}

internal sealed class AuditEntryRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public Guid ActorId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
