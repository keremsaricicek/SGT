using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Ports;

/// <summary>Sistemdeki ekranların listesi, ait oldukları ürün ve ekranda tanımlı eylemler.</summary>
public interface IScreenCatalog
{
    IReadOnlyList<ScreenDefinition> All { get; }
}

/// <summary>Bir ekran. Her ekranda "view" vardır; <paramref name="Actions"/> ek eylemleri listeler.</summary>
public sealed record ScreenDefinition(string Code, Product Product, IReadOnlyList<string> Actions)
{
    public string Area => Code.Split('.')[0];

    public IEnumerable<PermissionCode> AllPermissions() =>
        new[] { PermissionCode.ViewAction }.Concat(Actions).Select(a => PermissionCode.Parse($"{Code}:{a}").Value);

    public bool Defines(PermissionCode code)
    {
        ArgumentNullException.ThrowIfNull(code);
        return code.Screen == Code && (code.IsView || Actions.Contains(code.Action));
    }
}

public static class ScreenCatalogExtensions
{
    public static bool Defines(this IScreenCatalog catalog, PermissionCode code)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        return catalog.All.Any(s => s.Defines(code));
    }
}
