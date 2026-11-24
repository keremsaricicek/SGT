using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;

namespace Sgt.Modules.Configuration.Application.ExchangeRates;

public sealed record GetCurrentRatesQuery(TenantId TenantId);

/// <summary>Her yabancı birim için şu an geçerli kur (Spec 004 KK7). Kuru olmayan birim listede yer almaz.</summary>
public sealed class GetCurrentRatesHandler(IExchangeRateStore rates, IClock clock)
    : IQueryHandler<GetCurrentRatesQuery, IReadOnlyList<ExchangeRateDto>>
{
    public async Task<IReadOnlyList<ExchangeRateDto>> Handle(GetCurrentRatesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var now = clock.UtcNow;
        var current = new List<ExchangeRateDto>();
        foreach (var currency in SupportedCurrencies.All.Where(c => c != SupportedCurrencies.Base).Order(StringComparer.Ordinal))
        {
            var rate = await rates.LatestBefore(query.TenantId, currency, now, ct);
            if (rate is not null)
            {
                current.Add(new ExchangeRateDto(rate.Currency, rate.Buy, rate.Sell, rate.EffectiveFrom));
            }
        }

        return current;
    }
}
