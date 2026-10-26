namespace Sgt.BuildingBlocks;

/// <summary>Hatanın türü; API bunu HTTP durum koduna çevirir.</summary>
public enum ErrorKind
{
    Validation,
    NotFound,
    Forbidden,
    Conflict,
}

/// <summary>Beklenen bir iş kuralı ihlali. Kod makine için, mesaj insan içindir.</summary>
public sealed record Error(string Code, string Message, ErrorKind Kind = ErrorKind.Validation)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
