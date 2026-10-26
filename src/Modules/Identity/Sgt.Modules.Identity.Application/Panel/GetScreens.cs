using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Panel;

public sealed record GetScreensQuery(TenantId TenantId);

public sealed record ScreenView(string Code, string Product, IReadOnlyList<string> Actions);

/// <summary>Yetki panelinde gösterilecek ekranlar: yalnızca müşterinin lisansındaki ürünlerinki.</summary>
public sealed class GetScreensHandler(IScreenCatalog screens, ILicensedProducts licenses)
    : IQueryHandler<GetScreensQuery, IReadOnlyList<ScreenView>>
{
    public async Task<IReadOnlyList<ScreenView>> Handle(GetScreensQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var licensed = await licenses.For(query.TenantId, ct);
        return screens.All
            .Where(s => s.Product == Product.Core || licensed.Contains(s.Product))
            .Select(s => new ScreenView(s.Code, s.Product.ToString(), [PermissionCode.ViewAction, .. s.Actions]))
            .ToList();
    }
}
