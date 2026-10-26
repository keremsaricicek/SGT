namespace Sgt.BuildingBlocks;

/// <summary>Beklenen bir iş kuralı ihlali. Kod makine için, mesaj insan içindir.</summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
