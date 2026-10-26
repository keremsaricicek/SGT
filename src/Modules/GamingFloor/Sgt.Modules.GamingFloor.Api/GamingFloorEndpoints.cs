using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.GamingFloor.Application.Ratings;
using Sgt.Modules.GamingFloor.Application.Tables;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.GamingFloor.Domain;

namespace Sgt.Modules.GamingFloor.Api;

public sealed record DefineTableRequest(
    string Code, GameType GameType, string Currency, decimal MinBet, decimal MaxBet, decimal HouseEdgePercent, int HandsPerHour, string ReasonCode);

public sealed record FloorReasonRequest(string ReasonCode);

public sealed record StartRatingRequest(Guid TableId, Guid PlayerId, string ReasonCode);

public sealed record RatingValuesRequest(decimal AverageBet, decimal? PlayerResult, string ReasonCode);

/// <summary>Canlı oyun ekranları (Spec 007).</summary>
public static class GamingFloorEndpoints
{
    public static IEndpointRouteBuilder MapGamingFloorEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/floor").RequireAuthorization(SgtClaims.Policy).WithTags("GamingFloor");

        api.MapGet("/tables", async (IRequestContext request, IQueryHandler<ListTablesQuery, IReadOnlyList<TableView>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new ListTablesQuery(request.TenantId), ct)))
            .RequirePermission("live.tables:view");

        api.MapPost("/tables", async (DefineTableRequest body, IRequestContext request, ICommandHandler<DefineTableCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DefineTableCommand(request.TenantId, body.Code, body.GameType, body.Currency, body.MinBet, body.MaxBet, body.HouseEdgePercent, body.HandsPerHour, audit), ct))
                .ToHttp(id => Results.Created($"/api/floor/tables/{id}", new { id }))))
            .RequirePermission("live.tables:define");

        api.MapPost("/tables/{tableId:guid}/open", async (Guid tableId, FloorReasonRequest body, IRequestContext request, OpenTableSessionHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new TableSessionCommand(request.TenantId, tableId, audit), ct)).ToHttp()))
            .RequirePermission("live.table_session:open");

        api.MapPost("/tables/{tableId:guid}/close", async (Guid tableId, FloorReasonRequest body, IRequestContext request, CloseTableSessionHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new TableSessionCommand(request.TenantId, tableId, audit), ct)).ToHttp()))
            .RequirePermission("live.table_session:close");

        api.MapPost("/ratings", async (StartRatingRequest body, IRequestContext request, ICommandHandler<StartRatingCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new StartRatingCommand(request.TenantId, body.TableId, body.PlayerId, audit), ct))
                .ToHttp(id => Results.Created($"/api/floor/ratings/{id}", new { id }))))
            .RequirePermission("live.rating:start");

        api.MapPost("/ratings/{ratingId:guid}/finish", async (Guid ratingId, RatingValuesRequest body, IRequestContext request, ICommandHandler<FinishRatingCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new FinishRatingCommand(request.TenantId, ratingId, body.AverageBet, body.PlayerResult, audit), ct)).ToHttp()))
            .RequirePermission("live.rating:finish");

        api.MapPost("/ratings/{ratingId:guid}/correct", async (Guid ratingId, RatingValuesRequest body, IRequestContext request, ICommandHandler<CorrectRatingCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new CorrectRatingCommand(request.TenantId, ratingId, body.AverageBet, body.PlayerResult, audit), ct)).ToHttp()))
            .RequirePermission("live.rating:correct");

        api.MapGet("/players/{playerId:guid}/location", async (Guid playerId, IRequestContext request, IPlayerLocationQuery query, CancellationToken ct) =>
            await query.CurrentLocation(request.TenantId, playerId, ct) is { } location ? Results.Ok(location) : Results.NoContent())
            .RequirePermission("player.locator:view");

        return app;
    }

    private static async Task<IResult> WithReason(string? reasonCode, IRequestContext request, Func<AuditContext, Task<IResult>> run)
    {
        var reason = ReasonCode.Create(reasonCode);
        return reason.IsSuccess ? await run(request.Audit(reason.Value)) : ResultHttpExtensions.Problem(reason.Error);
    }
}
