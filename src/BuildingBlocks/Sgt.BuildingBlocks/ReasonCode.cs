using System.Text.Json.Serialization;

namespace Sgt.BuildingBlocks;

/// <summary>Bir işlemin neden yapıldığını belirten kod (ör. ROLE_SETUP).</summary>
public sealed record ReasonCode
{
    [JsonConstructor]
    private ReasonCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static readonly Error Required = new("Common.ReasonCodeRequired", "Gerekçe kodu zorunludur.");

    /// <summary>Kullanıcı girdisinden oluşturur; boşsa hata döner.</summary>
    public static Result<ReasonCode> Create(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Trim().Length > 50 ? Required : new ReasonCode(value.Trim().ToUpperInvariant());

    public static ReasonCode Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Gerekçe kodu boş olamaz.", nameof(value));
        }

        return new ReasonCode(value.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;
}
