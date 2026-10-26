using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Domain;

/// <summary>Çekmecede tutulan değer türü.</summary>
public enum Instrument
{
    Cash,
    Chips,
}

/// <summary>Defter hesabı: çekmece veya döviz takas (FX) hesabı.</summary>
public enum LedgerAccount
{
    Drawer,
    FxClearing,
}

/// <summary>Bir araç ve para birimindeki tutar (açılış ve sayım için).</summary>
public sealed record DrawerAmount(Instrument Instrument, string Currency, decimal Amount);

/// <summary>Tek defter satırı. İşaretli tutar: + çekmeceye/hesaba giriş, − çıkış. Değiştirilmez.</summary>
public sealed record LedgerEntry(LedgerAccount Account, Instrument Instrument, string Currency, decimal Amount);

public enum CageTransactionType
{
    ChipSale,
    ChipRedeem,
    ExchangeForeignToBase,
    ExchangeBaseToForeign,

    /// <summary>Başka bir işlemin ters kaydı (Spec 010).</summary>
    Reversal,
}

/// <summary>
/// Kasa işlemi. Kimliği istemcinin verdiği idempotency anahtarıdır (KK5).
/// Kayıtlar para birimi bazında toplamı sıfırdır (KK7).
/// </summary>
public sealed class CageTransaction
{
    private CageTransaction(
        Guid id, CageTransactionType type, Guid playerId, Money amount, IReadOnlyList<LedgerEntry> entries, decimal? rate, AuditContext audit, DateTimeOffset occurredAt,
        Guid? reversesId = null)
    {
        ReversesId = reversesId;
        Id = id;
        Type = type;
        PlayerId = playerId;
        Amount = amount;
        Entries = entries;
        Rate = rate;
        Audit = audit;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; }

    public CageTransactionType Type { get; }

    public Guid PlayerId { get; }

    /// <summary>İstenen tutar: çip işleminde çip tutarı, döviz işleminde yabancı para tutarı.</summary>
    public Money Amount { get; }

    public IReadOnlyList<LedgerEntry> Entries { get; }

    /// <summary>Döviz işleminde kullanılan kur (1 yabancı birim = Rate TRY).</summary>
    public decimal? Rate { get; }

    public AuditContext Audit { get; }

    public DateTimeOffset OccurredAt { get; }

    /// <summary>Ters kayıtsa, ters çevirdiği işlem.</summary>
    public Guid? ReversesId { get; }

    internal static CageTransaction Create(
        Guid id, CageTransactionType type, Guid playerId, Money amount, IReadOnlyList<LedgerEntry> entries, decimal? rate, AuditContext audit, DateTimeOffset at,
        Guid? reversesId = null)
    {
        if (entries.GroupBy(e => e.Currency).Any(g => g.Sum(e => e.Amount) != 0m))
        {
            throw new InvalidOperationException("Dengesiz defter kaydı üretildi; bu bir yazılım hatasıdır.");
        }

        return new CageTransaction(id, type, playerId, amount, entries, rate, audit, at, reversesId);
    }

    public static CageTransaction Restore(
        Guid id, CageTransactionType type, Guid playerId, Money amount, IReadOnlyList<LedgerEntry> entries, decimal? rate, AuditContext audit, DateTimeOffset at,
        Guid? reversesId) =>
        new(id, type, playerId, amount, entries, rate, audit, at, reversesId);

    /// <summary>Aynı idempotency anahtarıyla gelen isteğin ilk istekle aynı olup olmadığı (KK5).</summary>
    public bool IsSameRequest(CageTransactionType type, Guid playerId, Money amount) =>
        Type == type && PlayerId == playerId && Amount == amount;
}

/// <summary>Kapanışta bir araç/birim için beklenen, sayılan ve fark.</summary>
public sealed record ClosingCount(Instrument Instrument, string Currency, decimal Expected, decimal Counted)
{
    public decimal Variance => Counted - Expected;
}

internal static class CageAmounts
{
    public static bool IsValidPositive(decimal amount) => amount > 0 && decimal.Round(amount, 2) == amount;

    public static bool IsValidNonNegative(decimal amount) => amount >= 0 && decimal.Round(amount, 2) == amount;
}
