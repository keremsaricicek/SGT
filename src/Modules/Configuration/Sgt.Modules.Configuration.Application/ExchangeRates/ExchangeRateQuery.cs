using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;

namespace Sgt.Modules.Configuration.Application.ExchangeRates;

public sealed class ExchangeRateQuery(IExchangeRateStore rates) : IExchangeRateQuery
{
    public async Task<ExchangeRateDto?> RateAt(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct)
    {
        if (!SupportedCurrencies.IsSupported(currency))
        {
            return null;
        }

        var rate = await rates.LatestBefore(tenantId, currency.Trim().ToUpperInvariant(), at, ct);
        return rate is null ? null : new ExchangeRateDto(rate.Currency, rate.Buy, rate.Sell, rate.EffectiveFrom);
    }
}
