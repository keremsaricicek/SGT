using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Modules.Configuration.Infrastructure.Persistence;

/// <summary>Configuration modülünün şeması. exchange_rates append-only'dir (Spec 004 KK3).</summary>
public sealed class ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : DbContext(options)
{
    public const string Schema = "configuration";

    internal DbSet<ExchangeRateRow> ExchangeRates => Set<ExchangeRateRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddOutboxAndInbox();

        modelBuilder.Entity<ExchangeRateRow>(e =>
        {
            e.ToTable("exchange_rates");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.Currency, x.EffectiveFrom });
            e.Property(x => x.Currency).HasMaxLength(3);
            e.Property(x => x.Buy).HasPrecision(18, 6);
            e.Property(x => x.Sell).HasPrecision(18, 6);
            e.Property(x => x.Terminal).HasMaxLength(100);
            e.Property(x => x.ReasonCode).HasMaxLength(50);
        });
    }
}

internal sealed class ExchangeRateRow
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal Buy { get; set; }

    public decimal Sell { get; set; }

    public DateTimeOffset EffectiveFrom { get; set; }

    public Guid ActorId { get; set; }

    public string Terminal { get; set; } = string.Empty;

    public string ReasonCode { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }
}
