using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Loyalty.Application.Adjustments;
using Sgt.Modules.Loyalty.Application.Comps;
using Sgt.Modules.Loyalty.Application.Rules;
using Sgt.Modules.Loyalty.Application.Summary;

namespace Sgt.Modules.Loyalty.Api;

/// <summary>RequestId: istemcinin ürettiği idempotency anahtarı; aynı anahtar ikinci istek açmaz.</summary>
public sealed record PointsRequest(Guid RequestId, Guid PlayerId, long Points, string Text, string ReasonCode);

public sealed record DecisionRequest(bool Approve, string ReasonCode);

public sealed record EarningRuleRequest(decimal TryPerPoint, string ReasonCode);

/// <summary>Loyalty ekranları (Spec 008).</summary>
public static class LoyaltyEndpoints
{
    public static IEndpointRouteBuilder MapLoyaltyEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1/loyalty").RequireAuthorization(SgtClaims.Policy).WithTags("Loyalty");

        api.MapGet("/players/{playerId:guid}", async (Guid playerId, IRequestContext request, IQueryHandler<GetLoyaltySummaryQuery, LoyaltySummary> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new GetLoyaltySummaryQuery(request.TenantId, playerId), ct)))
            .RequirePermission("loyalty.points:view");

        api.MapPost("/adjustments", async (PointsRequest body, IRequestContext request, ICommandHandler<RequestPointsAdjustmentCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new RequestPointsAdjustmentCommand(request.TenantId, body.RequestId, body.PlayerId, body.Points, body.Text, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/loyalty/adjustments/{id}", new { id }))))
            .RequirePermission("loyalty.points:adjust");

        api.MapPost("/adjustments/{requestId:guid}/decision", async (Guid requestId, DecisionRequest body, IRequestContext request, ICommandHandler<DecidePointsAdjustmentCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DecidePointsAdjustmentCommand(request.TenantId, requestId, body.Approve, audit), ct)).ToHttp()))
            .RequirePermission("loyalty.points:approve");

        api.MapPost("/comps", async (PointsRequest body, IRequestContext request, ICommandHandler<RequestCompCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new RequestCompCommand(request.TenantId, body.RequestId, body.PlayerId, body.Points, body.Text, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/loyalty/comps/{id}", new { id }))))
            .RequirePermission("loyalty.comp:request");

        api.MapPost("/comps/{requestId:guid}/decision", async (Guid requestId, DecisionRequest body, IRequestContext request, ICommandHandler<DecideCompCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DecideCompCommand(request.TenantId, requestId, body.Approve, audit), ct)).ToHttp()))
            .RequirePermission("loyalty.comp:approve");

        api.MapPut("/rules/earning", async (EarningRuleRequest body, IRequestContext request, ICommandHandler<SetEarningRuleCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new SetEarningRuleCommand(request.TenantId, body.TryPerPoint, audit), ct)).ToHttp()))
            .RequirePermission("loyalty.rules:define");

        return app;
    }

    private static async Task<IResult> WithReason(string? reasonCode, IRequestContext request, Func<AuditContext, Task<IResult>> run)
    {
        var reason = ReasonCode.Create(reasonCode);
        return reason.IsSuccess ? await run(request.Audit(reason.Value)) : ResultHttpExtensions.Problem(reason.Error);
    }
}
