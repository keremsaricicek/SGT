using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Domain;
using Sgt.Modules.GamingFloor.Domain.Slots;

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

public interface ISlotMachineRepository
{
    Task<SlotMachine?> Get(TenantId tenantId, Guid machineId, CancellationToken ct);

    Task<SlotMachine?> GetByAsset(TenantId tenantId, string assetNumber, CancellationToken ct);

    Task<bool> AssetExists(TenantId tenantId, string assetNumber, CancellationToken ct);

    Task<bool> ReadingExists(TenantId tenantId, ReadingSource source, string reference, CancellationToken ct);

    Task<IReadOnlyList<SlotMachine>> List(TenantId tenantId, CancellationToken ct);

    Task<IReadOnlyList<RecordedReading>> ReadingsBetween(TenantId tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    Task<IReadOnlyList<RecordedReading>> LatestReadings(TenantId tenantId, Guid machineId, int take, CancellationToken ct);

    void Add(SlotMachine machine);

    void AddReading(RecordedReading reading);
}

/// <summary>Bir aggregate'e bağlı olmayan denetim olayları (ör. içe aktarma özeti); commit ile yazılır.</summary>
public interface IFloorEvents
{
    void Publish(TenantId tenantId, FloorChange change);
}

public interface IFloorInbox : IInbox
{
}

public interface IFloorUnitOfWork : IUnitOfWork
{
}
