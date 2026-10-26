using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Modules.Finance.Infrastructure.Persistence;

/// <summary>
/// Finance modülünün şeması. İşlem, defter, açılış ve sayım satırları append-only'dir; defter satırları
/// işlem ve para birimi bazında dengeli olmak zorundadır (veritabanı tetikleyicileri, Spec 006 KK7).
/// </summary>
public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public const string Schema = "finance";

    internal DbSet<ShiftRow> Shifts => Set<ShiftRow>();

    internal DbSet<CageTransactionRow> Transactions => Set<CageTransactionRow>();

    internal DbSet<AdjustmentRequestRow> AdjustmentRequests => Set<AdjustmentRequestRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddOutboxAndInbox();

        modelBuilder.Entity<ShiftRow>(e =>
        {
            e.ToTable("cashier_shifts");
            e.HasKey(s => s.Id);
            e.Property(s => s.Terminal).HasMaxLength(100);
            e.Property(s => s.ClosingNote).HasMaxLength(1000);
            e.Property(s => s.Version).IsRowVersion();
            e.HasIndex(s => new { s.TenantId, s.CashierId }).IsUnique().HasFilter("closed_at IS NULL");
            e.HasIndex(s => new { s.TenantId, s.Terminal }).IsUnique().HasFilter("closed_at IS NULL");
            e.HasMany(s => s.Opening).WithOne().HasForeignKey(o => o.ShiftId);
            e.HasMany(s => s.Transactions).WithOne().HasForeignKey(t => t.ShiftId);
            e.HasMany(s => s.ClosingCounts).WithOne().HasForeignKey(c => c.ShiftId);
        });

        modelBuilder.Entity<OpeningAmountRow>(e =>
        {
            e.ToTable("shift_opening_amounts");
            e.HasKey(o => new { o.ShiftId, o.Instrument, o.Currency });
            e.Property(o => o.Instrument).HasMaxLength(10);
            e.Property(o => o.Currency).HasMaxLength(3);
            e.Property(o => o.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<CageTransactionRow>(e =>
        {
            e.ToTable("cage_transactions");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).ValueGeneratedNever();
            e.HasIndex(t => new { t.TenantId, t.OccurredAt });
            e.HasIndex(t => new { t.TenantId, t.PlayerId });
            e.Property(t => t.Type).HasMaxLength(30);
            e.Property(t => t.Currency).HasMaxLength(3);
            e.Property(t => t.Amount).HasPrecision(18, 2);
            e.Property(t => t.Rate).HasPrecision(18, 6);
            e.Property(t => t.Terminal).HasMaxLength(100);
            e.Property(t => t.ReasonCode).HasMaxLength(50);
            e.HasIndex(t => t.ReversesId).IsUnique().HasFilter("reverses_id IS NOT NULL"); // bir işlem bir kez ters çevrilir
            e.HasMany(t => t.Entries).WithOne().HasForeignKey(l => l.TransactionId);
        });

        modelBuilder.Entity<LedgerEntryRow>(e =>
        {
            e.ToTable("ledger_entries");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).ValueGeneratedNever();
            e.Property(l => l.Account).HasMaxLength(20);
            e.Property(l => l.Instrument).HasMaxLength(10);
            e.Property(l => l.Currency).HasMaxLength(3);
            e.Property(l => l.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<AdjustmentRequestRow>(e =>
        {
            e.ToTable("cage_adjustment_requests");
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.TenantId, r.ShiftId });
            e.Property(r => r.Note).HasMaxLength(1000);
            e.Property(r => r.Status).HasMaxLength(20);
            e.Property(r => r.Version).IsRowVersion();
        });

        modelBuilder.Entity<ClosingCountRow>(e =>
        {
            e.ToTable("shift_closing_counts");
            e.HasKey(c => new { c.ShiftId, c.Instrument, c.Currency });
            e.Property(c => c.Instrument).HasMaxLength(10);
            e.Property(c => c.Currency).HasMaxLength(3);
            e.Property(c => c.Expected).HasPrecision(18, 2);
            e.Property(c => c.Counted).HasPrecision(18, 2);
        });
    }
}

internal sealed class ShiftRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid CashierId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public DateTimeOffset OpenedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public string? ClosingNote { get; set; }

    public uint Version { get; set; }

    public List<OpeningAmountRow> Opening { get; set; } = [];

    public List<CageTransactionRow> Transactions { get; set; } = [];

    public List<ClosingCountRow> ClosingCounts { get; set; } = [];
}

internal sealed class OpeningAmountRow
{
    public Guid ShiftId { get; set; }

    public string Instrument { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}

internal sealed class CageTransactionRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ShiftId { get; set; }

    public string Type { get; set; } = string.Empty;

    public Guid PlayerId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal? Rate { get; set; }

    public Guid ActorId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public Guid? ReversesId { get; set; }

    public List<LedgerEntryRow> Entries { get; set; } = [];
}

internal sealed class LedgerEntryRow
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public string Account { get; set; } = string.Empty;

    public string Instrument { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}

internal sealed class ClosingCountRow
{
    public Guid ShiftId { get; set; }

    public string Instrument { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal Expected { get; set; }

    public decimal Counted { get; set; }
}

internal sealed class AdjustmentRequestRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ShiftId { get; set; }

    public Guid TransactionId { get; set; }

    public string Note { get; set; } = string.Empty;

    public DateTimeOffset RequestedAt { get; set; }

    public Guid RequestedBy { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? DecidedBy { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public Guid? ReversalId { get; set; }

    public uint Version { get; set; }
}
