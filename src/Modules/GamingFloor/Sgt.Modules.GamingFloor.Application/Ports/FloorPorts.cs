using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Domain;

namespace Sgt.Modules.GamingFloor.Application.Ports;

public interface ITableRepository
{
    Task<GamingTable?> Get(TenantId tenantId, Guid tableId, CancellationToken ct);

    Task<bool> CodeExists(TenantId tenantId, string code, CancellationToken ct);

    Task<IReadOnlyList<GamingTable>> List(TenantId tenantId, CancellationToken ct);

    void Add(GamingTable table);
}

public interface IRatingRepository
{
    Task<Rating?> Get(TenantId tenantId, Guid ratingId, CancellationToken ct);

    Task<Rating?> RunningForPlayer(TenantId tenantId, Guid playerId, CancellationToken ct);

    Task<int> RunningAtTable(TenantId tenantId, Guid tableId, CancellationToken ct);

    void Add(Rating rating);
}

public interface IFloorUnitOfWork : IUnitOfWork
{
}
