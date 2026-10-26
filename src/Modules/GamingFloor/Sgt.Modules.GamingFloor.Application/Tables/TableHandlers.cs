using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Domain;

namespace Sgt.Modules.GamingFloor.Application.Tables;

public sealed record DefineTableCommand(
    TenantId TenantId, string Code, GameType GameType, string Currency, decimal MinBet, decimal MaxBet, decimal HouseEdgePercent, int HandsPerHour,
    AuditContext Audit);

/// <summary>Masa tanımı (Spec 007 KK1).</summary>
public sealed class DefineTableHandler(ITableRepository tables, IFloorUnitOfWork unitOfWork, IClock clock) : ICommandHandler<DefineTableCommand, Guid>
{
    public async Task<Result<Guid>> Handle(DefineTableCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var table = GamingTable.Define(
            Guid.NewGuid(), command.TenantId, command.Code, command.GameType, command.Currency, command.MinBet, command.MaxBet,
            command.HouseEdgePercent, command.HandsPerHour, command.Audit, clock.UtcNow);
        if (table.IsFailure)
        {
            return table.Error;
        }

        if (await tables.CodeExists(command.TenantId, table.Value.Code, ct))
        {
            return FloorErrors.TableCodeTaken;
        }

        tables.Add(table.Value);
        await unitOfWork.Commit(ct);
        return table.Value.Id;
    }
}

public sealed record TableSessionCommand(TenantId TenantId, Guid TableId, AuditContext Audit);

/// <summary>Masayı açar (Spec 007 KK2).</summary>
public sealed class OpenTableSessionHandler(ITableRepository tables, IFloorUnitOfWork unitOfWork, IClock clock) : ICommandHandler<TableSessionCommand>
{
    public async Task<Result> Handle(TableSessionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var table = await tables.Get(command.TenantId, command.TableId, ct);
        if (table is null)
        {
            return FloorErrors.TableNotFound;
        }

        var result = table.Open(command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}

/// <summary>Masayı kapatır; süren rating varken kapanmaz (Spec 007 KK2).</summary>
public sealed class CloseTableSessionHandler(ITableRepository tables, IRatingRepository ratings, IFloorUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<TableSessionCommand>
{
    public async Task<Result> Handle(TableSessionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var table = await tables.Get(command.TenantId, command.TableId, ct);
        if (table is null)
        {
            return FloorErrors.TableNotFound;
        }

        var result = table.Close(await ratings.RunningAtTable(command.TenantId, table.Id, ct), command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}

public sealed record ListTablesQuery(TenantId TenantId);

public sealed record TableView(
    Guid TableId, string Code, string GameType, string Currency, decimal MinBet, decimal MaxBet, decimal HouseEdgePercent, int HandsPerHour, bool IsOpen);

public sealed class ListTablesHandler(ITableRepository tables) : IQueryHandler<ListTablesQuery, IReadOnlyList<TableView>>
{
    public async Task<IReadOnlyList<TableView>> Handle(ListTablesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return (await tables.List(query.TenantId, ct))
            .OrderBy(t => t.Code, StringComparer.Ordinal)
            .Select(t => new TableView(t.Id, t.Code, t.GameType.ToString(), t.Currency, t.MinBet, t.MaxBet, t.HouseEdgePercent, t.HandsPerHour, t.IsOpen))
            .ToList();
    }
}
