namespace Sgt.BuildingBlocks;

/// <summary>KKTC pazarında desteklenen para birimleri (anayasa, Ek Kısıtlar). Ana birim TRY.</summary>
public static class SupportedCurrencies
{
    public const string Base = "TRY";

    public static readonly IReadOnlyList<string> All = ["TRY", "EUR", "GBP", "USD"];

    public static bool IsSupported(string? currency) =>
        currency is not null && All.Contains(currency.Trim().ToUpperInvariant());
}
