using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Access;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Application.Menu;

public sealed record GetMyMenuQuery(TenantId TenantId, UserId UserId);

/// <summary>Menüde görünen ekran ve kullanıcının o ekranda yapabileceği eylemler.</summary>
public sealed record MenuItem(string Screen, IReadOnlyList<string> Actions);

/// <summary>Kullanıcının menüsü: yalnızca görme yetkisi olan ve lisansı açık ekranlar, katalog sırasıyla (KK4).</summary>
public sealed class GetMyMenuHandler(EffectiveAccess access, IScreenCatalog screens)
    : IQueryHandler<GetMyMenuQuery, IReadOnlyList<MenuItem>>
{
    public async Task<IReadOnlyList<MenuItem>> Handle(GetMyMenuQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var granted = await access.For(query.TenantId, query.UserId, ct);
        var visibleScreens = granted.Where(c => c.IsView).Select(c => c.Screen).ToHashSet();

        return screens.All
            .Where(s => visibleScreens.Contains(s.Code))
            .Select(s => new MenuItem(
                s.Code,
                granted.Where(c => c.Screen == s.Code).Select(c => c.Action).Order(StringComparer.Ordinal).ToList()))
            .ToList();
    }
}
