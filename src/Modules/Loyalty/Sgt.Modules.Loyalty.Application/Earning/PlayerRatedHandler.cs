using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Earning;

/// <summary>
/// Biten/düzeltilen rating'den puan kazandırır (Spec 008 KK1–KK2). Aynı olay iki kez işlenmez (inbox).
/// TRY dışı rating için kur yoksa hata fırlatır; olay yeniden denenir ve gerekirse operatöre düşer.
/// </summary>
public sealed class PlayerRatedHandler(
    ILoyaltyAccountRepository accounts,
    IEarningRuleStore rules,
    IExchangeRateQuery rates,
    ILoyaltyInbox inbox,
    ILoyaltyUnitOfWork unitOfWork) : IIntegrationEventHandler<PlayerRatedV1>
{
    private const string HandlerName = nameof(PlayerRatedHandler);

    public async Task Handle(PlayerRatedV1 integrationEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        var e = integrationEvent;
        if (await inbox.AlreadyProcessed(e.EventId, HandlerName, ct))
        {
            return;
        }

        var theoreticalWinTry = await ToBaseCurrency(e, ct);
        var rule = await rules.Current(e.TenantId, ct);

        var account = await accounts.Get(e.TenantId, e.PlayerId, ct);
        if (account is null)
        {
            account = LoyaltyAccount.Open(e.TenantId, e.PlayerId);
            accounts.Add(account);
        }

        account.EarnFromRating(e.RatingId, rule.PointsFor(theoreticalWinTry), e.Audit, e.OccurredAt);
        inbox.MarkProcessed(e.EventId, HandlerName);
        await unitOfWork.Commit(ct);
    }

    private async Task<decimal> ToBaseCurrency(PlayerRatedV1 e, CancellationToken ct)
    {
        if (e.Currency == SupportedCurrencies.Base)
        {
            return e.TheoreticalWin;
        }

        var rate = await rates.RateAt(e.TenantId, e.Currency, e.FinishedAt, ct)
            ?? throw new InvalidOperationException($"{e.Currency} için {e.FinishedAt:u} anında kur yok; puan hesaplanamadı.");
        return e.TheoreticalWin * rate.Buy;
    }
}
