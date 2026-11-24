using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Application.Panel;

public sealed record GetUserAccessQuery(TenantId TenantId, UserId UserId);

/// <summary>Bir kullanıcının yetki tablosu: roller, kişiye eklenen/çıkarılan ve sonuçta geçerli yetkiler.</summary>
public sealed record UserAccessView(
    Guid UserId,
    bool IsSuperAdmin,
    IReadOnlyList<Guid> RoleIds,
    IReadOnlyList<string> Grants,
    IReadOnlyList<string> Denied,
    IReadOnlyList<string> Effective);

public sealed class GetUserAccessHandler(IUserAccessRepository users, IRoleRepository roles)
    : IQueryHandler<GetUserAccessQuery, UserAccessView?>
{
    public async Task<UserAccessView?> Handle(GetUserAccessQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var user = await users.Get(query.TenantId, query.UserId, ct);
        if (user is null)
        {
            return null;
        }

        var assignedRoles = await roles.GetMany(query.TenantId, user.RoleIds, ct);
        return new UserAccessView(
            user.UserId.Value,
            user.IsSuperAdmin,
            user.RoleIds.Select(r => r.Value).Order().ToList(),
            PanelFormat.Codes(user.Grants.Codes),
            PanelFormat.Codes(user.Denied),
            PanelFormat.Codes(user.EffectivePermissions(assignedRoles)));
    }
}
