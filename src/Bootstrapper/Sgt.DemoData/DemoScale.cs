namespace Sgt.DemoData;

/// <summary>Demo verisinin hacmi (Spec 012 KK3). Testler Small ile çalışır.</summary>
public sealed record DemoScale(string Name, int Players, int Tables, int SlotMachines, int Days)
{
    public static readonly DemoScale Full = new("Full", 500, 40, 200, 30);

    public static readonly DemoScale Small = new("Small", 20, 4, 8, 3);

    /// <summary>Yapılandırma değeri (<c>DemoSeed:Scale</c>); boşsa Full.</summary>
    public static DemoScale Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Full;
        }

        return new[] { Full, Small }.SingleOrDefault(s => string.Equals(s.Name, value.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Bilinmeyen demo ölçeği: {value}. Geçerli: Full, Small.", nameof(value));
    }
}
