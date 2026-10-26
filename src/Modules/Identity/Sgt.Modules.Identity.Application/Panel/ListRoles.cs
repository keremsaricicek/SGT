using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Application.Panel;

public sealed record ListRolesQuery(TenantId TenantId);

public sealed record RoleView(Guid Id, string Name, IReadOnlyList<string> Permissions);

public sealed class ListRolesHandler(IRoleRepository roles) : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleView>>
{
    public async Task<IReadOnlyList<RoleView>> Handle(ListRolesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var all = await roles.List(query.TenantId, ct);
        return all
            .OrderBy(r => r.Name, StringComparer.CurrentCulture)
            .Select(r => new RoleView(r.Id.Value, r.Name, PanelFormat.Codes(r.Permissions.Codes)))
            .ToList();
    }
}
