using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Application.Panel;

public sealed record ListUsersQuery(TenantId TenantId);

public sealed record UserSummary(Guid UserId, string DisplayName, string EmployeeCode, bool IsActive, bool IsSuperAdmin);

/// <summary>Personel listesi (Spec 003 KK7), ada göre sıralı.</summary>
public sealed class ListUsersHandler(IUserAccessRepository users) : IQueryHandler<ListUsersQuery, IReadOnlyList<UserSummary>>
{
    public async Task<IReadOnlyList<UserSummary>> Handle(ListUsersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var all = await users.List(query.TenantId, ct);
        return all
            .OrderBy(u => u.DisplayName, StringComparer.CurrentCulture)
            .Select(u => new UserSummary(u.UserId.Value, u.DisplayName, u.EmployeeCode, u.IsActive, u.IsSuperAdmin))
            .ToList();
    }
}
