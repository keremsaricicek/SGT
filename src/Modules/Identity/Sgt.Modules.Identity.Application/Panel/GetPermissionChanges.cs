using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;

namespace Sgt.Modules.Identity.Application.Panel;

/// <summary>Yetki değişiklik geçmişi, en yeniden eskiye. <paramref name="Before"/> ile sayfalanır.</summary>
public sealed record GetPermissionChangesQuery(TenantId TenantId, int Take, DateTimeOffset? Before);

public sealed record PermissionChangeView(
    Guid Id,
    string Target,
    Guid TargetId,
    string Kind,
    string Item,
    Guid ActorId,
    string Terminal,
    string ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed class GetPermissionChangesHandler(IPermissionChangeReader reader)
    : IQueryHandler<GetPermissionChangesQuery, IReadOnlyList<PermissionChangeView>>
{
    public const int MaxTake = 200;

    public Task<IReadOnlyList<PermissionChangeView>> Handle(GetPermissionChangesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return reader.Recent(query.TenantId, Math.Clamp(query.Take, 1, MaxTake), query.Before, ct);
    }
}
