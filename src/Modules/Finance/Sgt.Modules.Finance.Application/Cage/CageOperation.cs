using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

/// <summary>Kasa işleminin sonucu; çekmecedeki hareketler istemciye gösterilir.</summary>
public sealed record CageTransactionResult(
    Guid TransactionId, string Type, decimal Amount, string Currency, decimal? Rate, IReadOnlyList<DrawerMovement> Movements)
{
    public static CageTransactionResult From(CageTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return new(
            transaction.Id,
            transaction.Type.ToString(),
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Rate,
            transaction.Entries
                .Where(e => e.Account == LedgerAccount.Drawer)
                .Select(e => new DrawerMovement(e.Instrument.ToString(), e.Currency, e.Amount))
                .ToList());
    }
}

public sealed record DrawerMovement(string Instrument, string Currency, decimal Amount);

/// <summary>
/// Tüm kasa işlemlerinin ortak adımları: idempotency (KK5), oyuncu kontrolü (KK6), vardiyayı yükleme, kaydetme.
/// İşleme özel kural her işlevin kendi handler'ındadır.
/// </summary>
public sealed class CageOperation(IShiftRepository shifts, IPlayerStatusQuery players, IFinanceUnitOfWork unitOfWork, IClock clock)
{
    public DateTimeOffset Now => clock.UtcNow;

    public async Task<Result<CageTransactionResult>> Run(
        TenantId tenantId,
        Guid shiftId,
        Guid transactionId,
        Guid playerId,
        CageTransactionType type,
        Money amount,
        Func<CashierShift, Task<Result<CageTransaction>>> execute,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(execute);
        var existing = await shifts.FindTransaction(tenantId, transactionId, ct);
        if (existing is not null)
        {
            return existing.IsSameRequest(type, playerId, amount) ? CageTransactionResult.From(existing) : FinanceErrors.DuplicateKeyMismatch;
        }

        var player = await players.GetStatus(tenantId, playerId, ct);
        if (player is null)
        {
            return FinanceErrors.PlayerNotFound;
        }

        if (player.IsRestricted)
        {
            return FinanceErrors.PlayerRestricted;
        }

        var shift = await shifts.Get(tenantId, shiftId, ct);
        if (shift is null)
        {
            return FinanceErrors.ShiftNotFound;
        }

        var transaction = await execute(shift);
        if (transaction.IsFailure)
        {
            return transaction.Error;
        }

        await unitOfWork.Commit(ct);
        return CageTransactionResult.From(transaction.Value);
    }
}
