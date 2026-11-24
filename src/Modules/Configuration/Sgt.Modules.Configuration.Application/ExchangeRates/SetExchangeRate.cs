using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Modules.Configuration.Application.ExchangeRates;

public sealed record SetExchangeRateCommand(TenantId TenantId, string Currency, decimal Buy, decimal Sell, AuditContext Audit);

/// <summary>Yeni kur girer (Spec 004 KK2, KK5).</summary>
public sealed class SetExchangeRateHandler(
    IExchangeRateStore rates,
    IConfigurationOutbox outbox,
    IConfigurationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetExchangeRateCommand>
{
    public async Task<Result> Handle(SetExchangeRateCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var rate = ExchangeRate.Set(command.TenantId, command.Currency, command.Buy, command.Sell, command.Audit, clock.UtcNow);
        if (rate.IsFailure)
        {
            return rate;
        }

        var r = rate.Value;
        rates.Add(r);
        outbox.Add(new ExchangeRateSetV1(r.Id, r.Audit.CorrelationId, r.EffectiveFrom, r.TenantId, r.Audit, r.Currency, r.Buy, r.Sell));
        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
