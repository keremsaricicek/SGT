using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Tests;

public sealed class CashierShiftTests
{
    internal static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    internal static readonly UserId Cashier = new(Guid.Parse("00000000-0000-0000-0000-0000000000c1"));
    internal static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
    internal static readonly Guid Player = Guid.Parse("00000000-0000-0000-0000-0000000000f1");

    internal static AuditContext Audit(UserId? actor = null) =>
        new(actor ?? Cashier, "KASA-1", ReasonCode.Of("CAGE"), Guid.NewGuid());

    internal static CashierShift OpenShift() =>
        CashierShift.Open(Guid.NewGuid(), Tenant, "KASA-1",
        [
            new DrawerAmount(Instrument.Cash, "TRY", 10_000m),
            new DrawerAmount(Instrument.Chips, "TRY", 50_000m),
            new DrawerAmount(Instrument.Cash, "EUR", 500m),
        ],
        Audit(), Now).Value;

    [Fact]
    public void KK1_open_records_opening_balances()
    {
        var shift = OpenShift();

        Assert.Equal(10_000m, shift.Balance(Instrument.Cash, "TRY"));
        Assert.Equal(50_000m, shift.Balance(Instrument.Chips, "TRY"));
        Assert.Equal(0m, shift.Balance(Instrument.Cash, "USD"));
        Assert.Equal(Cashier, shift.CashierId);
        Assert.True(shift.IsOpen);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10.123)]
    public void KK1_KK4_opening_amounts_must_be_valid(decimal amount)
    {
        Assert.True(CashierShift.Open(Guid.NewGuid(), Tenant, "KASA-1", [new DrawerAmount(Instrument.Cash, "TRY", amount)], Audit(), Now).IsFailure);
    }

    [Fact]
    public void KK2_chip_sale_moves_cash_in_and_chips_out_and_is_balanced()
    {
        var shift = OpenShift();

        var tx = shift.SellChips(Guid.NewGuid(), Player, Money.Of(2_000m, "TRY"), Audit(), Now).Value;

        Assert.Equal(12_000m, shift.Balance(Instrument.Cash, "TRY"));
        Assert.Equal(48_000m, shift.Balance(Instrument.Chips, "TRY"));
        Assert.All(tx.Entries.GroupBy(e => e.Currency), g => Assert.Equal(0m, g.Sum(e => e.Amount)));
    }

    [Fact]
    public void KK2_redeem_requires_enough_cash()
    {
        var shift = OpenShift();

        Assert.Equal(FinanceErrors.InsufficientDrawer, shift.RedeemChips(Guid.NewGuid(), Player, Money.Of(10_000.01m, "TRY"), Audit(), Now).Error);
        Assert.True(shift.RedeemChips(Guid.NewGuid(), Player, Money.Of(10_000m, "TRY"), Audit(), Now).IsSuccess);
        Assert.Equal(0m, shift.Balance(Instrument.Cash, "TRY"));
    }

    [Fact]
    public void KK3_foreign_to_base_pays_rounded_down_at_buy_rate()
    {
        var shift = OpenShift();

        var tx = shift.ExchangeForeignToBase(Guid.NewGuid(), Player, Money.Of(100.5m, "EUR"), buyRate: 36.123456m, Audit(), Now).Value;

        // 100.5 × 36.123456 = 3630.407328 → oyuncuya 3630.40
        Assert.Equal(600.5m, shift.Balance(Instrument.Cash, "EUR"));
        Assert.Equal(10_000m - 3630.40m, shift.Balance(Instrument.Cash, "TRY"));
        Assert.Equal(36.123456m, tx.Rate);
        Assert.All(tx.Entries.GroupBy(e => e.Currency), g => Assert.Equal(0m, g.Sum(e => e.Amount)));
    }

    [Fact]
    public void KK3_base_to_foreign_charges_rounded_up_at_sell_rate_and_needs_foreign_cash()
    {
        var shift = OpenShift();

        Assert.Equal(FinanceErrors.InsufficientDrawer,
            shift.ExchangeBaseToForeign(Guid.NewGuid(), Player, Money.Of(501m, "EUR"), sellRate: 37m, Audit(), Now).Error);

        shift.ExchangeBaseToForeign(Guid.NewGuid(), Player, Money.Of(100m, "EUR"), sellRate: 37.000001m, Audit(), Now);

        // 100 × 37.000001 = 3700.0001 → oyuncudan 3700.01
        Assert.Equal(400m, shift.Balance(Instrument.Cash, "EUR"));
        Assert.Equal(13_700.01m, shift.Balance(Instrument.Cash, "TRY"));
    }

    [Theory]
    [InlineData(0, "TRY")]
    [InlineData(10.555, "TRY")]
    [InlineData(10, "JPY")]
    public void KK4_invalid_amounts_are_rejected(decimal amount, string currency)
    {
        Assert.True(OpenShift().SellChips(Guid.NewGuid(), Player, Money.Of(amount, currency), Audit(), Now).IsFailure);
    }

    [Fact]
    public void KK3_exchange_rejects_base_currency_as_foreign()
    {
        Assert.Equal(FinanceErrors.ForeignCurrencyRequired,
            OpenShift().ExchangeForeignToBase(Guid.NewGuid(), Player, Money.Of(10m, "TRY"), 1m, Audit(), Now).Error);
    }

    [Fact]
    public void KK8_only_the_cashier_operates_and_closed_shift_takes_no_transactions()
    {
        var shift = OpenShift();

        Assert.Equal(FinanceErrors.NotShiftOwner, shift.SellChips(Guid.NewGuid(), Player, Money.Of(10m, "TRY"), Audit(UserId.New()), Now).Error);

        shift.Close([new DrawerAmount(Instrument.Cash, "TRY", 10_000m), new DrawerAmount(Instrument.Chips, "TRY", 50_000m), new DrawerAmount(Instrument.Cash, "EUR", 500m)], null, Audit(), Now);

        Assert.Equal(FinanceErrors.ShiftClosed, shift.SellChips(Guid.NewGuid(), Player, Money.Of(10m, "TRY"), Audit(), Now).Error);
    }

    [Fact]
    public void KK9_close_records_variance_and_requires_note_when_not_zero()
    {
        var shift = OpenShift();
        shift.SellChips(Guid.NewGuid(), Player, Money.Of(1_000m, "TRY"), Audit(), Now);
        DrawerAmount[] counted = [new(Instrument.Cash, "TRY", 10_990m), new(Instrument.Chips, "TRY", 49_000m), new(Instrument.Cash, "EUR", 500m)];

        Assert.Equal(FinanceErrors.VarianceNoteRequired, shift.Close(counted, " ", Audit(), Now).Error);
        Assert.True(shift.Close(counted, "10 TL eksik, sayım tekrarlandı", Audit(), Now.AddHours(8)).IsSuccess);

        var cashTry = shift.ClosingCounts.Single(c => c.Instrument == Instrument.Cash && c.Currency == "TRY");
        Assert.Equal(11_000m, cashTry.Expected);
        Assert.Equal(-10m, cashTry.Variance);
        Assert.False(shift.IsOpen);
    }

    [Fact]
    public void KK9_uncounted_balances_are_treated_as_zero_count()
    {
        var shift = OpenShift();

        Assert.Equal(FinanceErrors.VarianceNoteRequired, shift.Close([new DrawerAmount(Instrument.Cash, "TRY", 10_000m)], null, Audit(), Now).Error);
    }

    [Fact]
    public void KK11_open_transactions_and_close_are_recorded_for_audit()
    {
        var shift = OpenShift();
        shift.SellChips(Guid.NewGuid(), Player, Money.Of(10m, "TRY"), Audit(), Now);

        Assert.Equal([ShiftChangeKind.Opened, ShiftChangeKind.TransactionRecorded], shift.PendingChanges.Select(c => c.Kind));
    }
}
