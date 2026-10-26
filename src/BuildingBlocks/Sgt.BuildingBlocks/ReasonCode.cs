namespace Sgt.BuildingBlocks;

/// <summary>Bir işlemin neden yapıldığını belirten kod (ör. ROLE_SETUP).</summary>
public sealed record ReasonCode
{
    private ReasonCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

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
