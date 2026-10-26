using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.GamingFloor.Application.Ratings;
using Sgt.Modules.GamingFloor.Application.Slots;
using Sgt.Modules.GamingFloor.Application.Tables;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.GamingFloor.Domain;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Api;

public sealed record DefineTableRequest(
    string Code, GameType GameType, string Currency, decimal MinBet, decimal MaxBet, decimal HouseEdgePercent, int HandsPerHour, string ReasonCode);

public sealed record FloorReasonRequest(string ReasonCode);

public sealed record StartRatingRequest(Guid TableId, Guid PlayerId, string ReasonCode);

public sealed record RatingValuesRequest(decimal AverageBet, decimal? PlayerResult, string ReasonCode);

public sealed record DefineSlotMachineRequest(string AssetNumber, string Area, string Manufacturer, string Model, decimal Denomination, string Currency, string ReasonCode);

/// <summary>Reference: istemcinin okuma kimliği (idempotency).</summary>
public sealed record MeterReadingRequest(
    string AssetNumber, DateTimeOffset ReadAt, decimal CoinIn, decimal CoinOut, decimal Jackpot, long GamesPlayed, bool IsReset, string? Note, string Reference, string ReasonCode);

public sealed record ImportRequest(string Csv, string ReasonCode);

/// <summary>Canlı oyun ekranları (Spec 007).</summary>
public static class GamingFloorEndpoints
{
    public static IEndpointRouteBuilder MapGamingFloorEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1/floor").RequireAuthorization(SgtClaims.Policy).WithTags("GamingFloor");

        api.MapGet("/tables", async (IRequestContext request, IQueryHandler<ListTablesQuery, IReadOnlyList<TableView>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new ListTablesQuery(request.TenantId), ct)))
            .RequirePermission("live.tables:view")
            .Produces<IReadOnlyList<TableView>>(StatusCodes.Status200OK)
            .ProducesQueryProblems();

        api.MapPost("/tables", async (DefineTableRequest body, IRequestContext request, ICommandHandler<DefineTableCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DefineTableCommand(request.TenantId, body.Code, body.GameType, body.Currency, body.MinBet, body.MaxBet, body.HouseEdgePercent, body.HandsPerHour, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/floor/tables/{id}", new CreatedId(id)))))
            .RequirePermission("live.tables:define")
            .ProducesCreatedCommand();

        api.MapPost("/tables/{tableId:guid}/open", async (Guid tableId, FloorReasonRequest body, IRequestContext request, OpenTableSessionHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new TableSessionCommand(request.TenantId, tableId, audit), ct)).ToHttp()))
            .RequirePermission("live.table_session:open")
            .ProducesNoContentCommand();

        api.MapPost("/tables/{tableId:guid}/close", async (Guid tableId, FloorReasonRequest body, IRequestContext request, CloseTableSessionHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new TableSessionCommand(request.TenantId, tableId, audit), ct)).ToHttp()))
            .RequirePermission("live.table_session:close")
            .ProducesNoContentCommand();

        api.MapPost("/ratings", async (StartRatingRequest body, IRequestContext request, ICommandHandler<StartRatingCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new StartRatingCommand(request.TenantId, body.TableId, body.PlayerId, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/floor/ratings/{id}", new CreatedId(id)))))
            .RequirePermission("live.rating:start")
            .ProducesCreatedCommand();

        api.MapPost("/ratings/{ratingId:guid}/finish", async (Guid ratingId, RatingValuesRequest body, IRequestContext request, ICommandHandler<FinishRatingCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new FinishRatingCommand(request.TenantId, ratingId, body.AverageBet, body.PlayerResult, audit), ct)).ToHttp()))
            .RequirePermission("live.rating:finish")
            .ProducesNoContentCommand();

        api.MapPost("/ratings/{ratingId:guid}/correct", async (Guid ratingId, RatingValuesRequest body, IRequestContext request, ICommandHandler<CorrectRatingCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new CorrectRatingCommand(request.TenantId, ratingId, body.AverageBet, body.PlayerResult, audit), ct)).ToHttp()))
            .RequirePermission("live.rating:correct")
            .ProducesNoContentCommand();

        api.MapGet("/players/{playerId:guid}/location", async (Guid playerId, IRequestContext request, IPlayerLocationQuery query, CancellationToken ct) =>
            await query.CurrentLocation(request.TenantId, playerId, ct) is { } location ? Results.Ok(location) : Results.NoContent())
            .RequirePermission("player.locator:view")
            .Produces<PlayerLocationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesQueryProblems();

        var slots = api.MapGroup("/slots");

        slots.MapGet("/", async (IRequestContext request, IQueryHandler<GetSlotFloorQuery, IReadOnlyList<SlotFloorRow>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new GetSlotFloorQuery(request.TenantId), ct)))
            .RequirePermission("slot.floor:view")
            .Produces<IReadOnlyList<SlotFloorRow>>(StatusCodes.Status200OK)
            .ProducesQueryProblems();

        slots.MapGet("/{machineId:guid}", async (Guid machineId, IRequestContext request, IQueryHandler<GetSlotMachineQuery, SlotMachineView?> handler, CancellationToken ct) =>
            await handler.Handle(new GetSlotMachineQuery(request.TenantId, machineId), ct) is { } view ? Results.Ok(view) : ResultHttpExtensions.Problem(SlotErrors.MachineNotFound))
            .RequirePermission("slot.machine:view")
            .Produces<SlotMachineView>(StatusCodes.Status200OK)
            .ProducesQueryProblems()
            .ProducesProblem(StatusCodes.Status404NotFound);

        slots.MapPost("/", async (DefineSlotMachineRequest body, IRequestContext request, ICommandHandler<DefineSlotMachineCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DefineSlotMachineCommand(request.TenantId, body.AssetNumber, body.Area, body.Manufacturer, body.Model, body.Denomination, body.Currency, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/floor/slots/{id}", new CreatedId(id)))))
            .RequirePermission("slot.machines_setup:define")
            .ProducesCreatedCommand();

        slots.MapPost("/readings", async (MeterReadingRequest body, IRequestContext request, ICommandHandler<EnterMeterReadingCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new EnterMeterReadingCommand(
                    request.TenantId, body.AssetNumber, body.ReadAt, new MeterSnapshot(body.CoinIn, body.CoinOut, body.Jackpot, body.GamesPlayed),
                    body.IsReset, body.Note, body.Reference, audit), ct)).ToHttp()))
            .RequirePermission("slot.manual_entry:enter")
            .ProducesNoContentCommand();

        slots.MapPost("/readings/import", async (ImportRequest body, IRequestContext request, ICommandHandler<ImportMeterReadingsCommand, ImportReport> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new ImportMeterReadingsCommand(request.TenantId, body.Csv, audit), ct)).ToHttp(Results.Ok)))
            .RequirePermission("slot.manual_entry:import")
            .Produces<ImportReport>(StatusCodes.Status200OK)
            .ProducesCommandProblems();

        return app;
    }

    private static async Task<IResult> WithReason(string? reasonCode, IRequestContext request, Func<AuditContext, Task<IResult>> run)
    {
        var reason = ReasonCode.Create(reasonCode);
        return reason.IsSuccess ? await run(request.Audit(reason.Value)) : ResultHttpExtensions.Problem(reason.Error);
    }
}
