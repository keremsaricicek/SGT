using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Modules.Configuration.Application.Ports;

public interface IExchangeRateStore
{
    void Add(ExchangeRate rate);

    Task<ExchangeRate?> LatestBefore(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct);
}

/// <summary>Olayı modülün outbox'ına ekler; unit of work ile aynı transaction'da yazılır.</summary>
public interface IConfigurationOutbox
{
    void Add(IIntegrationEvent integrationEvent);
}

public interface IConfigurationUnitOfWork : IUnitOfWork
{
}
