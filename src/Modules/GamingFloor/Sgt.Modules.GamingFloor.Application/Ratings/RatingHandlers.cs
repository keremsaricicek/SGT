using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.GamingFloor.Domain;

namespace Sgt.Modules.GamingFloor.Application.Ratings;

public sealed record StartRatingCommand(TenantId TenantId, Guid TableId, Guid PlayerId, AuditContext Audit);

/// <summary>Rating başlatır (Spec 007 KK3).</summary>
public sealed class StartRatingHandler(
    ITableRepository tables, IRatingRepository ratings, IPlayerStatusQuery players, IFloorUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<StartRatingCommand, Guid>
{
    public async Task<Result<Guid>> Handle(StartRatingCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var table = await tables.Get(command.TenantId, command.TableId, ct);
        if (table is null)
        {
            return FloorErrors.TableNotFound;
        }

        if (!table.IsOpen)
        {
            return FloorErrors.TableNotOpen;
        }

        var player = await players.GetStatus(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return FloorErrors.PlayerNotFound;
        }

        if (player.IsRestricted)
        {
            return FloorErrors.PlayerRestricted;
        }

        if (await ratings.RunningForPlayer(command.TenantId, command.PlayerId, ct) is not null)
        {
            return FloorErrors.PlayerAlreadyRated;
        }

        var rating = Rating.Start(Guid.NewGuid(), table, command.PlayerId, command.Audit, clock.UtcNow);
        ratings.Add(rating);
        await unitOfWork.Commit(ct);
        return rating.Id;
    }
}

public sealed record FinishRatingCommand(TenantId TenantId, Guid RatingId, decimal AverageBet, decimal? PlayerResult, AuditContext Audit);

/// <summary>Rating'i bitirir, teorik kazancı hesaplar (Spec 007 KK4–KK5).</summary>
public sealed class FinishRatingHandler(ITableRepository tables, IRatingRepository ratings, IFloorUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<FinishRatingCommand>
{
    public async Task<Result> Handle(FinishRatingCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var rating = await ratings.Get(command.TenantId, command.RatingId, ct);
        var table = rating is null ? null : await tables.Get(command.TenantId, rating.TableId, ct);
        if (rating is null || table is null)
        {
            return FloorErrors.RatingNotFound;
        }

        var result = rating.Finish(table, command.AverageBet, command.PlayerResult, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}

public sealed record CorrectRatingCommand(TenantId TenantId, Guid RatingId, decimal AverageBet, decimal? PlayerResult, AuditContext Audit);

/// <summary>Bitmiş rating'i düzeltir; eski değerler korunur (Spec 007 KK6).</summary>
public sealed class CorrectRatingHandler(ITableRepository tables, IRatingRepository ratings, IFloorUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CorrectRatingCommand>
{
    public async Task<Result> Handle(CorrectRatingCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var rating = await ratings.Get(command.TenantId, command.RatingId, ct);
        var table = rating is null ? null : await tables.Get(command.TenantId, rating.TableId, ct);
        if (rating is null || table is null)
        {
            return FloorErrors.RatingNotFound;
        }

        var result = rating.Correct(table, command.AverageBet, command.PlayerResult, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}

/// <summary>Oyuncunun şu an hangi masada olduğu (Spec 007 KK7).</summary>
public sealed class PlayerLocationQuery(IRatingRepository ratings) : IPlayerLocationQuery
{
    public async Task<PlayerLocationDto?> CurrentLocation(TenantId tenantId, Guid playerId, CancellationToken ct)
    {
        var rating = await ratings.RunningForPlayer(tenantId, playerId, ct);
        return rating is null ? null : new PlayerLocationDto(playerId, rating.TableCode, rating.StartedAt);
    }
}

public sealed record ListRunningRatingsQuery(TenantId TenantId, Guid? TableId);

public sealed record RunningRatingView(Guid RatingId, Guid PlayerId, Guid TableId, string TableCode, DateTimeOffset StartedAt);

/// <summary>Süren rating'ler (pit ekranı ve Windows istemcisi yeniden açıldığında).</summary>
public sealed class ListRunningRatingsHandler(IRatingRepository ratings) : IQueryHandler<ListRunningRatingsQuery, IReadOnlyList<RunningRatingView>>
{
    public async Task<IReadOnlyList<RunningRatingView>> Handle(ListRunningRatingsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return (await ratings.Running(query.TenantId, query.TableId, ct))
            .Select(r => new RunningRatingView(r.Id, r.PlayerId, r.TableId, r.TableCode, r.StartedAt))
            .ToList();
    }
}
