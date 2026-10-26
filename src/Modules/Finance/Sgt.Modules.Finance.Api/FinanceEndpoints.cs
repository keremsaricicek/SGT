using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Finance.Application.Cage;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Api;

public sealed record OpenShiftRequest(IReadOnlyList<DrawerAmount> Opening, string ReasonCode);

/// <summary>TransactionId: istemcinin her işlem için ürettiği idempotency anahtarı (KK5).</summary>
public sealed record CageRequest(Guid TransactionId, Guid PlayerId, decimal Amount, string Currency, string ReasonCode);

public sealed record AdjustmentRequest(Guid TransactionId, string Note, string ReasonCode);

public sealed record AdjustmentDecisionRequest(bool Approve, string ReasonCode);

public sealed record CloseShiftRequest(IReadOnlyList<DrawerAmount> Counted, string? Note, string ReasonCode);

/// <summary>Kasa ekranları (Spec 006). Para hareketi uçları insan uzman finans onayına tabidir.</summary>
public static class FinanceEndpoints
{
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1/cage/shifts").RequireAuthorization(SgtClaims.Policy).WithTags("Cage");

        api.MapPost("/", async (OpenShiftRequest body, IRequestContext request, ICommandHandler<OpenShiftCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new OpenShiftCommand(request.TenantId, body.Opening, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/cage/shifts/{id}", new CreatedId(id)))))
            .RequirePermission("cage.shift:open")
            .ProducesCreatedCommand();

        api.MapGet("/{shiftId:guid}", async (Guid shiftId, IRequestContext request, IQueryHandler<GetShiftSummaryQuery, ShiftSummary?> handler, CancellationToken ct) =>
            await handler.Handle(new GetShiftSummaryQuery(request.TenantId, shiftId), ct) is { } summary
                ? Results.Ok(summary)
                : ResultHttpExtensions.Problem(FinanceErrors.ShiftNotFound))
            .RequirePermission("cage.summary:view")
            .Produces<ShiftSummary>(StatusCodes.Status200OK)
            .ProducesQueryProblems()
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapGet("/mine", async (IRequestContext request, IQueryHandler<GetMyOpenShiftQuery, Guid?> handler, CancellationToken ct) =>
            await handler.Handle(new GetMyOpenShiftQuery(request.TenantId, request.UserId), ct) is { } id ? Results.Ok(new CreatedId(id)) : Results.NoContent())
            .RequirePermission("cage.shift:view")
            .Produces<CreatedId>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesQueryProblems();

        api.MapPost("/{shiftId:guid}/chip-sales", async (Guid shiftId, CageRequest body, IRequestContext request, SellChipsHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new ChipCommand(request.TenantId, shiftId, body.TransactionId, body.PlayerId, body.Amount, body.Currency, audit), ct))
                .ToHttp(Results.Ok)))
            .RequirePermission("cage.chips:sell")
            .Produces<CageTransactionResult>(StatusCodes.Status200OK)
            .ProducesCommandProblems();

        api.MapPost("/{shiftId:guid}/chip-redemptions", async (Guid shiftId, CageRequest body, IRequestContext request, RedeemChipsHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new ChipCommand(request.TenantId, shiftId, body.TransactionId, body.PlayerId, body.Amount, body.Currency, audit), ct))
                .ToHttp(Results.Ok)))
            .RequirePermission("cage.chips:redeem")
            .Produces<CageTransactionResult>(StatusCodes.Status200OK)
            .ProducesCommandProblems();

        api.MapPost("/{shiftId:guid}/exchanges/foreign-to-base", async (Guid shiftId, CageRequest body, IRequestContext request, ExchangeForeignToBaseHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new ExchangeCommand(request.TenantId, shiftId, body.TransactionId, body.PlayerId, body.Amount, body.Currency, audit), ct))
                .ToHttp(Results.Ok)))
            .RequirePermission("cage.exchange:exchange")
            .Produces<CageTransactionResult>(StatusCodes.Status200OK)
            .ProducesCommandProblems();

        api.MapPost("/{shiftId:guid}/exchanges/base-to-foreign", async (Guid shiftId, CageRequest body, IRequestContext request, ExchangeBaseToForeignHandler handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new ExchangeCommand(request.TenantId, shiftId, body.TransactionId, body.PlayerId, body.Amount, body.Currency, audit), ct))
                .ToHttp(Results.Ok)))
            .RequirePermission("cage.exchange:exchange")
            .Produces<CageTransactionResult>(StatusCodes.Status200OK)
            .ProducesCommandProblems();

        api.MapPost("/{shiftId:guid}/close", async (Guid shiftId, CloseShiftRequest body, IRequestContext request, ICommandHandler<CloseShiftCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new CloseShiftCommand(request.TenantId, shiftId, body.Counted, body.Note, audit), ct)).ToHttp()))
            .RequirePermission("cage.shift:close")
            .ProducesNoContentCommand();

        api.MapPost("/{shiftId:guid}/adjustments", async (Guid shiftId, AdjustmentRequest body, IRequestContext request, ICommandHandler<RequestCageAdjustmentCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new RequestCageAdjustmentCommand(request.TenantId, shiftId, body.TransactionId, body.Note, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/cage/adjustments/{id}", new CreatedId(id)))))
            .RequirePermission("cage.adjustment:request")
            .ProducesCreatedCommand();

        app.MapPost("/api/v1/cage/adjustments/{requestId:guid}/decision", async (Guid requestId, AdjustmentDecisionRequest body, IRequestContext request, ICommandHandler<DecideCageAdjustmentCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DecideCageAdjustmentCommand(request.TenantId, requestId, body.Approve, audit), ct)).ToHttp()))
            .RequireAuthorization(SgtClaims.Policy)
            .RequirePermission("cage.adjustment:approve")
            .WithTags("Cage")
            .ProducesNoContentCommand();

        return app;
    }

    private static async Task<IResult> WithReason(string? reasonCode, IRequestContext request, Func<AuditContext, Task<IResult>> run)
    {
        var reason = ReasonCode.Create(reasonCode);
        return reason.IsSuccess ? await run(request.Audit(reason.Value)) : ResultHttpExtensions.Problem(reason.Error);
    }
}
