namespace Sgt.BuildingBlocks;

/// <summary>
/// Tutar ve para birimi birlikte. Para için float/double kullanılmaz;
/// farklı para birimleri örtük olarak toplanmaz.
/// </summary>
public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    /// <summary>ISO 4217 kodu (ör. TRY, EUR, GBP, USD).</summary>
    public string Currency { get; }

    public static Money Of(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new ArgumentException("ISO 4217 para birimi gerekli.", nameof(currency));
        }

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => Of(0m, currency);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other.Currency != Currency)
        {
            throw new InvalidOperationException($"Para birimi uyuşmuyor: {Currency} / {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Negate() => new(-Amount, Currency);
}
