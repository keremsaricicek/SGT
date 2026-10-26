namespace Sgt.Modules.CasinoPlayer.Domain;

/// <summary>Oyuncuya verilmiş bir kart. İptal edilen kart tekrar açılmaz.</summary>
public sealed class PlayerCard
{
    internal PlayerCard(CardNumber number, DateTimeOffset issuedAt, DateTimeOffset? cancelledAt = null)
    {
        Number = number;
        IssuedAt = issuedAt;
        CancelledAt = cancelledAt;
    }

    public CardNumber Number { get; }

    public DateTimeOffset IssuedAt { get; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public bool IsActive => CancelledAt is null;

    public static PlayerCard Restore(CardNumber number, DateTimeOffset issuedAt, DateTimeOffset? cancelledAt) =>
        new(number, issuedAt, cancelledAt);

    internal void Cancel(DateTimeOffset at) => CancelledAt ??= at;
}
