using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Domain;

public readonly record struct PlayerId(Guid Value)
{
    public static PlayerId New() => new(Guid.NewGuid());
}

/// <summary>Okunabilir oyuncu numarası: P + 6 hane (tenant içinde artan).</summary>
public sealed record PlayerNumber
{
    private PlayerNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PlayerNumber FromSequence(long sequence) =>
        sequence > 0 ? new($"P{sequence:D6}") : throw new ArgumentOutOfRangeException(nameof(sequence));

    public static PlayerNumber Restore(string value) => new(value);

    public override string ToString() => Value;
}

/// <summary>Oyuncu kartı numarası: 4–20 harf/rakam, büyük harfe çevrilir.</summary>
public sealed record CardNumber
{
    private CardNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CardNumber> Parse(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
        return normalized.Length is >= 4 and <= 20 && normalized.All(char.IsAsciiLetterOrDigit)
            ? new CardNumber(normalized)
            : PlayerErrors.InvalidCardNumber;
    }

    public override string ToString() => Value;
}

public enum DocumentType
{
    NationalId,
    Passport,
    DriverLicense,
}

public enum RestrictionType
{
    /// <summary>İşletme kararıyla yasaklı.</summary>
    Banned,

    /// <summary>Oyuncunun kendi talebiyle dışlama.</summary>
    SelfExcluded,
}
