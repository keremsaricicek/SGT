namespace Sgt.Windows.Core.Shared;

/// <summary>Desteklenen para birimi (TRY/EUR/GBP/USD). Serbest metin yerine değer nesnesi (K1).</summary>
public sealed record Currency
{
    public static readonly Currency Try = new("TRY");
    public static readonly Currency Eur = new("EUR");
    public static readonly Currency Gbp = new("GBP");
    public static readonly Currency Usd = new("USD");

    private Currency(string code) => Code = code;

    public static IReadOnlyList<Currency> All { get; } = [Try, Eur, Gbp, Usd];

    public string Code { get; }

    public static Currency? Find(string? code)
    {
        var normalized = code?.Trim().ToUpperInvariant();
        return All.FirstOrDefault(c => c.Code == normalized);
    }

    public override string ToString() => Code;
}
