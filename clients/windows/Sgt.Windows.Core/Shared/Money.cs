using System.Globalization;

namespace Sgt.Windows.Core.Shared;

/// <summary>Tutar + para birimi. Para için float/double kullanılmaz (K1).</summary>
public sealed record Money(decimal Amount, Currency Currency)
{
    /// <summary>Ekran girdisinden pozitif tutar oluşturur (ör. "1.250,50" TR, "1,250.50" EN).</summary>
    public static Money? ParsePositive(string? text, Currency? currency, IFormatProvider culture) =>
        currency is not null && Amounts.TryParse(text, culture, out var amount) && amount > 0
            ? new Money(amount, currency)
            : null;

    public override string ToString() => string.Create(CultureInfo.CurrentCulture, $"{Amount:N2} {Currency.Code}");
}

/// <summary>Ekrandan girilen sayıları çözer (sıfır ve negatif dahil; anlamını çağıran belirler).</summary>
public static class Amounts
{
    public static bool TryParse(string? text, IFormatProvider culture, out decimal amount)
    {
        amount = 0;
        return !string.IsNullOrWhiteSpace(text)
            && decimal.TryParse(text.Trim(), NumberStyles.Number, culture, out amount);
    }
}
