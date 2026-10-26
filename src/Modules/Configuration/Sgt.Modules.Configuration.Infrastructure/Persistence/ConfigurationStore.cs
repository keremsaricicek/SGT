using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Modules.Configuration.Infrastructure.Persistence;

internal sealed class ExchangeRateStore(ConfigurationDbContext db) : IExchangeRateStore
{
    public void Add(ExchangeRate rate) => db.ExchangeRates.Add(new ExchangeRateRow
    {
        Id = rate.Id,
        TenantId = rate.TenantId.Value,
        Currency = rate.Currency,
        Buy = rate.Buy,
        Sell = rate.Sell,
        EffectiveFrom = rate.EffectiveFrom,
        ActorId = rate.Audit.Actor.Value,
        Terminal = rate.Audit.Terminal,
        ReasonCode = rate.Audit.Reason.Value,
        CorrelationId = rate.Audit.CorrelationId,
    });

    public async Task<ExchangeRate?> LatestBefore(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct)
    {
        var row = await db.ExchangeRates.AsNoTracking()
            .Where(r => r.TenantId == tenantId.Value && r.Currency == currency && r.EffectiveFrom <= at)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync(ct);

        return row is null
            ? null
            : ExchangeRate.Restore(row.Id, new TenantId(row.TenantId), row.Currency, row.Buy, row.Sell, row.EffectiveFrom,
                new AuditContext(new UserId(row.ActorId), row.Terminal, ReasonCode.Of(row.ReasonCode), row.CorrelationId));
    }
}

internal sealed class ConfigurationOutbox(ConfigurationDbContext db, IntegrationEventSerializer serializer) : IConfigurationOutbox
{
    public void Add(IIntegrationEvent integrationEvent) => db.Set<OutboxMessage>().Add(serializer.ToOutbox(integrationEvent));
}

internal sealed class ConfigurationUnitOfWork(ConfigurationDbContext db) : IConfigurationUnitOfWork
{
    public Task Commit(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class DesignTimeFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<ConfigurationDbContext>
{
    public ConfigurationDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseNpgsql("Host=localhost", o => o.MigrationsHistoryTable("__ef_migrations", ConfigurationDbContext.Schema))
            .UseSnakeCaseNamingConvention()
            .Options);
}
