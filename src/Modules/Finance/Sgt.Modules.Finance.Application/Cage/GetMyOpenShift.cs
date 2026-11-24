using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Application.Ports;

namespace Sgt.Modules.Finance.Application.Cage;

public sealed record GetMyOpenShiftQuery(TenantId TenantId, UserId Cashier);

/// <summary>Kasiyerin açık vardiyası; yoksa null. Windows istemcisi yeniden açıldığında kullanır.</summary>
public sealed class GetMyOpenShiftHandler(IShiftRepository shifts) : IQueryHandler<GetMyOpenShiftQuery, Guid?>
{
    public Task<Guid?> Handle(GetMyOpenShiftQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return shifts.FindOpenShiftId(query.TenantId, query.Cashier, ct);
    }
}
