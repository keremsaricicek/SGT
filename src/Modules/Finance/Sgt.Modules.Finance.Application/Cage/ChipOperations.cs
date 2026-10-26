using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

/// <summary>Çip satışı veya bozdurma isteği. TransactionId istemcinin idempotency anahtarıdır.</summary>
public sealed record ChipCommand(
    TenantId TenantId, Guid ShiftId, Guid TransactionId, Guid PlayerId, decimal Amount, string Currency, AuditContext Audit);

/// <summary>Oyuncuya çip satışı (Spec 006 KK2).</summary>
public sealed class SellChipsHandler(CageOperation operation) : ICommandHandler<ChipCommand, CageTransactionResult>
{
    public Task<Result<CageTransactionResult>> Handle(ChipCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var amount = Money.Of(command.Amount, command.Currency);
        return operation.Run(command.TenantId, command.ShiftId, command.TransactionId, command.PlayerId, CageTransactionType.ChipSale, amount,
            shift => Task.FromResult(shift.SellChips(command.TransactionId, command.PlayerId, amount, command.Audit, operation.Now)), ct);
    }
}

/// <summary>Oyuncunun çipini bozdurma (Spec 006 KK2).</summary>
public sealed class RedeemChipsHandler(CageOperation operation) : ICommandHandler<ChipCommand, CageTransactionResult>
{
    public Task<Result<CageTransactionResult>> Handle(ChipCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var amount = Money.Of(command.Amount, command.Currency);
        return operation.Run(command.TenantId, command.ShiftId, command.TransactionId, command.PlayerId, CageTransactionType.ChipRedeem, amount,
            shift => Task.FromResult(shift.RedeemChips(command.TransactionId, command.PlayerId, amount, command.Audit, operation.Now)), ct);
    }
}
