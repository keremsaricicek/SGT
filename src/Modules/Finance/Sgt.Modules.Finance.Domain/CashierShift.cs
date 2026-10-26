using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Domain;

/// <summary>
/// Kasiyer vardiyası (Spec 006). Bakiye = açılış + çekmece defter kayıtları. Kayıtlar yalnızca eklenir.
/// </summary>
public sealed class CashierShift
{
    private readonly List<DrawerAmount> _opening = [];
    private readonly List<CageTransaction> _transactions = [];
    private readonly List<ClosingCount> _closingCounts = [];
    private readonly List<ShiftChange> _pending = [];

    private CashierShift(Guid id, TenantId tenantId, UserId cashierId, string terminal, DateTimeOffset openedAt)
    {
        Id = id;
        TenantId = tenantId;
        CashierId = cashierId;
        Terminal = terminal;
        OpenedAt = openedAt;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public UserId CashierId { get; }

    public string Terminal { get; }

    public DateTimeOffset OpenedAt { get; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public string? ClosingNote { get; private set; }

    public bool IsOpen => ClosedAt is null;

    public IReadOnlyList<DrawerAmount> Opening => _opening;

    public IReadOnlyList<CageTransaction> Transactions => _transactions;

    public IReadOnlyList<ClosingCount> ClosingCounts => _closingCounts;

    public IReadOnlyList<ShiftChange> PendingChanges => _pending;

    public static Result<CashierShift> Open(
        Guid id, TenantId tenantId, string terminal, IEnumerable<DrawerAmount> opening, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var amounts = opening.ToList();
        if (amounts.Any(a => !SupportedCurrencies.IsSupported(a.Currency)))
        {
            return FinanceErrors.UnsupportedCurrency;
        }

        if (amounts.Any(a => !CageAmounts.IsValidNonNegative(a.Amount)))
        {
            return FinanceErrors.InvalidOpeningAmount;
        }

        var shift = new CashierShift(id, tenantId, audit.Actor, terminal, at);
        shift._opening.AddRange(amounts
            .GroupBy(a => (a.Instrument, Currency: a.Currency.ToUpperInvariant()))
            .Select(g => new DrawerAmount(g.Key.Instrument, g.Key.Currency, g.Sum(a => a.Amount))));
        shift.Record(ShiftChangeKind.Opened, null, terminal, audit, at);
        return shift;
    }

    public static CashierShift Restore(
        Guid id, TenantId tenantId, UserId cashierId, string terminal, DateTimeOffset openedAt, DateTimeOffset? closedAt, string? closingNote,
        IEnumerable<DrawerAmount> opening, IEnumerable<CageTransaction> transactions, IEnumerable<ClosingCount> closingCounts)
    {
        var shift = new CashierShift(id, tenantId, cashierId, terminal, openedAt) { ClosedAt = closedAt, ClosingNote = closingNote };
        shift._opening.AddRange(opening);
        shift._transactions.AddRange(transactions);
        shift._closingCounts.AddRange(closingCounts);
        return shift;
    }

    /// <summary>Çekmecedeki beklenen bakiye.</summary>
    public decimal Balance(Instrument instrument, string currency) =>
        _opening.Where(a => a.Instrument == instrument && a.Currency == currency).Sum(a => a.Amount)
        + _transactions.SelectMany(t => t.Entries)
            .Where(e => e.Account == LedgerAccount.Drawer && e.Instrument == instrument && e.Currency == currency)
            .Sum(e => e.Amount);

    /// <summary>Oyuncu nakit verir, çip alır (KK2).</summary>
    public Result<CageTransaction> SellChips(Guid transactionId, Guid playerId, Money amount, AuditContext audit, DateTimeOffset at) =>
        Execute(transactionId, CageTransactionType.ChipSale, playerId, amount, audit, at, rate: null, () => Build(
            Require(Instrument.Chips, amount.Currency, amount.Amount),
            () => [Drawer(Instrument.Cash, amount.Currency, amount.Amount), Drawer(Instrument.Chips, amount.Currency, -amount.Amount)]));

    /// <summary>Oyuncu çip verir, nakit alır (KK2).</summary>
    public Result<CageTransaction> RedeemChips(Guid transactionId, Guid playerId, Money amount, AuditContext audit, DateTimeOffset at) =>
        Execute(transactionId, CageTransactionType.ChipRedeem, playerId, amount, audit, at, rate: null, () => Build(
            Require(Instrument.Cash, amount.Currency, amount.Amount),
            () => [Drawer(Instrument.Chips, amount.Currency, amount.Amount), Drawer(Instrument.Cash, amount.Currency, -amount.Amount)]));

    /// <summary>Oyuncu yabancı para verir; alış kuruyla, aşağı yuvarlanmış TRY alır (KK3).</summary>
    public Result<CageTransaction> ExchangeForeignToBase(
        Guid transactionId, Guid playerId, Money foreign, decimal buyRate, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(foreign);
        var paid = decimal.Round(foreign.Amount * buyRate, 2, MidpointRounding.ToZero);
        return Execute(transactionId, CageTransactionType.ExchangeForeignToBase, playerId, foreign, audit, at, buyRate, () => Build(
            ForeignOnly(foreign) ?? Require(Instrument.Cash, SupportedCurrencies.Base, paid),
            () =>
            [
                Drawer(Instrument.Cash, foreign.Currency, foreign.Amount),
                Fx(foreign.Currency, -foreign.Amount),
                Fx(SupportedCurrencies.Base, paid),
                Drawer(Instrument.Cash, SupportedCurrencies.Base, -paid),
            ]));
    }

    /// <summary>Oyuncu yabancı para ister; satış kuruyla, yukarı yuvarlanmış TRY öder (KK3).</summary>
    public Result<CageTransaction> ExchangeBaseToForeign(
        Guid transactionId, Guid playerId, Money foreign, decimal sellRate, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(foreign);
        var received = decimal.Round(foreign.Amount * sellRate, 2, MidpointRounding.ToPositiveInfinity);
        return Execute(transactionId, CageTransactionType.ExchangeBaseToForeign, playerId, foreign, audit, at, sellRate, () => Build(
            ForeignOnly(foreign) ?? Require(Instrument.Cash, foreign.Currency, foreign.Amount),
            () =>
            [
                Drawer(Instrument.Cash, SupportedCurrencies.Base, received),
                Fx(SupportedCurrencies.Base, -received),
                Fx(foreign.Currency, foreign.Amount),
                Drawer(Instrument.Cash, foreign.Currency, -foreign.Amount),
            ]));
    }

    /// <summary>
    /// Sayımla kapatır (KK9). Bakiyesi veya sayımı olan her araç/birim için fark kaydedilir;
    /// sayılmayan bakiye sıfır sayılmış kabul edilir.
    /// </summary>
    public Result Close(IEnumerable<DrawerAmount> counted, string? note, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var allowed = EnsureOperable(audit);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var counts = counted.ToList();
        if (counts.Any(c => !SupportedCurrencies.IsSupported(c.Currency) || !CageAmounts.IsValidNonNegative(c.Amount)))
        {
            return FinanceErrors.InvalidOpeningAmount;
        }

        var keys = _opening.Select(a => (a.Instrument, a.Currency))
            .Concat(_transactions.SelectMany(t => t.Entries).Where(e => e.Account == LedgerAccount.Drawer).Select(e => (e.Instrument, e.Currency)))
            .Concat(counts.Select(c => (c.Instrument, Currency: c.Currency.ToUpperInvariant())))
            .Distinct()
            .OrderBy(k => k.Instrument).ThenBy(k => k.Currency, StringComparer.Ordinal)
            .ToList();

        var result = keys
            .Select(k => new ClosingCount(
                k.Instrument,
                k.Currency,
                Balance(k.Instrument, k.Currency),
                counts.Where(c => c.Instrument == k.Instrument && string.Equals(c.Currency, k.Currency, StringComparison.OrdinalIgnoreCase)).Sum(c => c.Amount)))
            .ToList();

        if (result.Any(c => c.Variance != 0) && string.IsNullOrWhiteSpace(note))
        {
            return FinanceErrors.VarianceNoteRequired;
        }

        _closingCounts.AddRange(result);
        ClosedAt = at;
        ClosingNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Record(ShiftChangeKind.Closed, null, string.Join(", ", result.Where(c => c.Variance != 0).Select(c => $"{c.Instrument} {c.Currency} fark {c.Variance}")), audit, at);
        return Result.Success();
    }

    public CageTransaction? FindTransaction(Guid transactionId) => _transactions.SingleOrDefault(t => t.Id == transactionId);

    public void ClearPendingChanges() => _pending.Clear();

    private Result<CageTransaction> Execute(
        Guid transactionId,
        CageTransactionType type,
        Guid playerId,
        Money amount,
        AuditContext audit,
        DateTimeOffset at,
        decimal? rate,
        Func<Result<IReadOnlyList<LedgerEntry>>> buildEntries)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(audit);
        var allowed = EnsureOperable(audit);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        if (!SupportedCurrencies.IsSupported(amount.Currency))
        {
            return FinanceErrors.UnsupportedCurrency;
        }

        if (!CageAmounts.IsValidPositive(amount.Amount))
        {
            return FinanceErrors.InvalidAmount;
        }

        var entries = buildEntries();
        if (entries.IsFailure)
        {
            return entries.Error;
        }

        var transaction = CageTransaction.Create(transactionId, type, playerId, Money.Of(amount.Amount, amount.Currency), entries.Value, rate, audit, at);
        _transactions.Add(transaction);
        Record(ShiftChangeKind.TransactionRecorded, transaction.Id, $"{type} {amount.Amount} {amount.Currency}", audit, at);
        return transaction;
    }

    private Result EnsureOperable(AuditContext audit)
    {
        if (!IsOpen)
        {
            return FinanceErrors.ShiftClosed;
        }

        return audit.Actor == CashierId ? Result.Success() : FinanceErrors.NotShiftOwner;
    }

    private Error? Require(Instrument instrument, string currency, decimal amount) =>
        Balance(instrument, currency) >= amount ? null : FinanceErrors.InsufficientDrawer;

    private static Error? ForeignOnly(Money money) =>
        money.Currency == SupportedCurrencies.Base ? FinanceErrors.ForeignCurrencyRequired : null;

    // Kural ihlali yoksa defter satırlarını üretir.
    private static Result<IReadOnlyList<LedgerEntry>> Build(Error? failure, Func<IReadOnlyList<LedgerEntry>> entries)
    {
        if (failure is not null)
        {
            return failure;
        }

        return Result.Success(entries());
    }

    private static LedgerEntry Drawer(Instrument instrument, string currency, decimal amount) =>
        new(LedgerAccount.Drawer, instrument, currency, amount);

    private static LedgerEntry Fx(string currency, decimal amount) =>
        new(LedgerAccount.FxClearing, Instrument.Cash, currency, amount);

    private void Record(ShiftChangeKind kind, Guid? transactionId, string detail, AuditContext audit, DateTimeOffset at) =>
        _pending.Add(new ShiftChange(Guid.NewGuid(), kind, transactionId, detail, audit, at));
}
