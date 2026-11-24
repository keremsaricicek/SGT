using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Domain;

/// <summary>Oyuncunun kişisel bilgileri. <see cref="Normalize"/> kurallara uygun, temizlenmiş kopyayı döner.</summary>
public sealed record PlayerDetails(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Nationality,
    DocumentType DocumentType,
    string DocumentNumber,
    string? Phone)
{
    public const int MinimumAge = 18;
    private const int MaxNameLength = 100;

    /// <summary>Ekranda gösterilecek belge numarası: yalnızca son 4 hane (KK4).</summary>
    public string MaskedDocumentNumber =>
        DocumentNumber.Length <= 4 ? new string('*', DocumentNumber.Length) : $"****{DocumentNumber[^4..]}";

    public Result<PlayerDetails> Normalize(DateOnly today)
    {
        var first = FirstName?.Trim() ?? string.Empty;
        var last = LastName?.Trim() ?? string.Empty;
        if (first.Length == 0 || last.Length == 0 || first.Length > MaxNameLength || last.Length > MaxNameLength)
        {
            return PlayerErrors.NameRequired;
        }

        var nationality = Nationality?.Trim().ToUpperInvariant() ?? string.Empty;
        if (nationality.Length != 2 || !nationality.All(char.IsAsciiLetterUpper))
        {
            return PlayerErrors.InvalidNationality;
        }

        var document = new string((DocumentNumber ?? string.Empty).Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
        if (document.Length == 0)
        {
            return PlayerErrors.DocumentRequired;
        }

        if (BirthDate.AddYears(MinimumAge) > today)
        {
            return PlayerErrors.Underage;
        }

        var phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();
        return this with { FirstName = first, LastName = last, Nationality = nationality, DocumentNumber = document, Phone = phone };
    }
}
