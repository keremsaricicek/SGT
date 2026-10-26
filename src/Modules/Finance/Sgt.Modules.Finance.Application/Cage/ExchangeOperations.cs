using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

/// <summary>Döviz bozum isteği. Amount ve Currency yabancı para tarafıdır.</summary>
public sealed record ExchangeCommand(
    TenantId TenantId, Guid ShiftId, Guid TransactionId, Guid PlayerId, decimal Amount, string Currency, AuditContext Audit);

/// <summary>Oyuncu yabancı para verir, alış kuruyla TRY alır (Spec 006 KK3).</summary>
public sealed class ExchangeForeignToBaseHandler(CageOperation operation, IExchangeRateQuery rates)
    : ICommandHandler<ExchangeCommand, CageTransactionResult>
{
    public Task<Result<CageTransactionResult>> Handle(ExchangeCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var foreign = Money.Of(command.Amount, command.Currency);
        return operation.Run(command.TenantId, command.ShiftId, command.TransactionId, command.PlayerId, CageTransactionType.ExchangeForeignToBase, foreign,
            async shift =>
            {
                var rate = await rates.RateAt(command.TenantId, foreign.Currency, operation.Now, ct);
                return rate is null
                    ? FinanceErrors.RateUnavailable
                    : shift.ExchangeForeignToBase(command.TransactionId, command.PlayerId, foreign, rate.Buy, command.Audit, operation.Now);
            }, ct);
    }
}

/// <summary>Oyuncu TRY verir, satış kuruyla yabancı para alır (Spec 006 KK3).</summary>
public sealed class ExchangeBaseToForeignHandler(CageOperation operation, IExchangeRateQuery rates)
    : ICommandHandler<ExchangeCommand, CageTransactionResult>
{
    public Task<Result<CageTransactionResult>> Handle(ExchangeCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var foreign = Money.Of(command.Amount, command.Currency);
        return operation.Run(command.TenantId, command.ShiftId, command.TransactionId, command.PlayerId, CageTransactionType.ExchangeBaseToForeign, foreign,
            async shift =>
            {
                var rate = await rates.RateAt(command.TenantId, foreign.Currency, operation.Now, ct);
                return rate is null
                    ? FinanceErrors.RateUnavailable
                    : shift.ExchangeBaseToForeign(command.TransactionId, command.PlayerId, foreign, rate.Sell, command.Audit, operation.Now);
            }, ct);
    }
}
