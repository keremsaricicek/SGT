using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Ports;

/// <summary>Sistemdeki ekranların listesi ve hangi ürüne ait oldukları.</summary>
public interface IScreenCatalog
{
    IReadOnlyList<ScreenDefinition> All { get; }
}

public sealed record ScreenDefinition(string Code, Product Product);
