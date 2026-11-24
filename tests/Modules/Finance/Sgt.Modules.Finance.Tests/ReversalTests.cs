using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Tests;

public sealed class ReversalTests
{
    private static readonly AuditContext Supervisor = new(UserId.New(), "KASA-SUP", ReasonCode.Of("CORRECTION"), Guid.NewGuid());

    [Fact]
    public void KK1_KK5_reversal_restores_drawer_and_keeps_original()
    {
        var shift = CashierShiftTests.OpenShift();
        var sale = shift.SellChips(Guid.NewGuid(), CashierShiftTests.Player, Money.Of(2_000m, "TRY"), CashierShiftTests.Audit(), CashierShiftTests.Now).Value;

        var reversal = shift.Reverse(sale.Id, Guid.NewGuid(), Supervisor, CashierShiftTests.Now).Value;

        Assert.Equal(10_000m, shift.Balance(Instrument.Cash, "TRY"));
        Assert.Equal(50_000m, shift.Balance(Instrument.Chips, "TRY"));
        Assert.Equal(sale.Id, reversal.ReversesId);
        Assert.Equal(CageTransactionType.Reversal, reversal.Type);
        Assert.Equal(sale.Amount, reversal.Amount);
        Assert.Equal(2, shift.Transactions.Count);
    }

    [Fact]
    public void KK3_transaction_is_reversed_once_and_reversal_cannot_be_reversed()
    {
        var shift = CashierShiftTests.OpenShift();
        var sale = shift.SellChips(Guid.NewGuid(), CashierShiftTests.Player, Money.Of(100m, "TRY"), CashierShiftTests.Audit(), CashierShiftTests.Now).Value;
        var reversal = shift.Reverse(sale.Id, Guid.NewGuid(), Supervisor, CashierShiftTests.Now).Value;

        Assert.Equal(FinanceErrors.AlreadyReversed, shift.Reverse(sale.Id, Guid.NewGuid(), Supervisor, CashierShiftTests.Now).Error);
        Assert.Equal(FinanceErrors.CannotReverseReversal, shift.Reverse(reversal.Id, Guid.NewGuid(), Supervisor, CashierShiftTests.Now).Error);
    }

    [Fact]
    public void KK4_reversal_needs_open_shift_and_enough_drawer()
    {
        var shift = CashierShiftTests.OpenShift();
        var redeem = shift.RedeemChips(Guid.NewGuid(), CashierShiftTests.Player, Money.Of(1_000m, "TRY"), CashierShiftTests.Audit(), CashierShiftTests.Now).Value;
        shift.SellChips(Guid.NewGuid(), CashierShiftTests.Player, Money.Of(51_000m, "TRY"), CashierShiftTests.Audit(), CashierShiftTests.Now); // çip kalmadı

        Assert.Equal(FinanceErrors.InsufficientDrawer, shift.Reverse(redeem.Id, Guid.NewGuid(), Supervisor, CashierShiftTests.Now).Error);
    }

    [Fact]
    public void KK2_request_cannot_be_approved_by_requester()
    {
        var shift = CashierShiftTests.OpenShift();
        var sale = shift.SellChips(Guid.NewGuid(), CashierShiftTests.Player, Money.Of(100m, "TRY"), CashierShiftTests.Audit(), CashierShiftTests.Now).Value;
        var request = CageAdjustmentRequest.Create(shift, sale.Id, "Yanlış tutar", CashierShiftTests.Audit(), CashierShiftTests.Now).Value;

        Assert.Equal(FourEyesDecision.SameUser, request.Decide(shift, approve: true, CashierShiftTests.Audit(), CashierShiftTests.Now).Error);
        Assert.True(request.Decide(shift, approve: true, Supervisor, CashierShiftTests.Now).IsSuccess);
        Assert.NotNull(request.ReversalId);
        Assert.Equal(10_000m, shift.Balance(Instrument.Cash, "TRY"));
    }

    [Fact]
    public void KK1_request_needs_note_and_existing_transaction()
    {
        var shift = CashierShiftTests.OpenShift();

        Assert.Equal(FinanceErrors.TransactionNotFound, CageAdjustmentRequest.Create(shift, Guid.NewGuid(), "not", CashierShiftTests.Audit(), CashierShiftTests.Now).Error);
        var sale = shift.SellChips(Guid.NewGuid(), CashierShiftTests.Player, Money.Of(100m, "TRY"), CashierShiftTests.Audit(), CashierShiftTests.Now).Value;
        Assert.Equal(FinanceErrors.AdjustmentNoteRequired, CageAdjustmentRequest.Create(shift, sale.Id, " ", CashierShiftTests.Audit(), CashierShiftTests.Now).Error);
    }
}
