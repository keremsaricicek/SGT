using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.CasinoPlayer.Application.Cards;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.CasinoPlayer.Application.Restrictions;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Api;

public sealed record PlayerDetailsRequest(
    string FirstName, string LastName, DateOnly BirthDate, string Nationality, DocumentType DocumentType, string DocumentNumber, string? Phone, string ReasonCode)
{
    public PlayerDetails ToDetails() => new(FirstName, LastName, BirthDate, Nationality, DocumentType, DocumentNumber, Phone);
}

public sealed record IssueCardRequest(string CardNumber, string ReasonCode);

public sealed record AddRestrictionRequest(RestrictionType Type, string? Note, DateTimeOffset? Until, string ReasonCode);

public sealed record PlayerReasonRequest(string ReasonCode);

/// <summary>Oyuncu ekranları (Spec 005). Her uç ilgili ekran yetkisini ister.</summary>
public static class CasinoPlayerEndpoints
{
    public static IEndpointRouteBuilder MapCasinoPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1/players").RequireAuthorization(SgtClaims.Policy).WithTags("Players");

        api.MapGet("/", async (string? q, IRequestContext request, IQueryHandler<SearchPlayersQuery, IReadOnlyList<PlayerSummary>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new SearchPlayersQuery(request.TenantId, q ?? string.Empty), ct)))
            .RequirePermission("player.search:view")
            .Produces<IReadOnlyList<PlayerSummary>>(StatusCodes.Status200OK)
            .ProducesQueryProblems();

        api.MapGet("/{playerId:guid}", async (Guid playerId, IRequestContext request, IQueryHandler<GetPlayerQuery, PlayerView?> handler, CancellationToken ct) =>
            await handler.Handle(new GetPlayerQuery(request.TenantId, new PlayerId(playerId)), ct) is { } view
                ? Results.Ok(view)
                : ResultHttpExtensions.Problem(PlayerErrors.PlayerNotFound))
            .RequirePermission("player.profile:view")
            .Produces<PlayerView>(StatusCodes.Status200OK)
            .ProducesQueryProblems()
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapPost("/", async (PlayerDetailsRequest body, IRequestContext request, ICommandHandler<RegisterPlayerCommand, PlayerRegistered> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new RegisterPlayerCommand(request.TenantId, body.ToDetails(), audit), ct))
                .ToHttp(r => Results.Created($"/api/v1/players/{r.PlayerId}", r))))
            .RequirePermission("player.profile:create")
            .Produces<PlayerRegistered>(StatusCodes.Status201Created)
            .ProducesCommandProblems();

        api.MapPut("/{playerId:guid}", async (Guid playerId, PlayerDetailsRequest body, IRequestContext request, ICommandHandler<UpdatePlayerDetailsCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new UpdatePlayerDetailsCommand(request.TenantId, new PlayerId(playerId), body.ToDetails(), audit), ct)).ToHttp()))
            .RequirePermission("player.profile:edit")
            .ProducesNoContentCommand();

        api.MapPost("/{playerId:guid}/cards", async (Guid playerId, IssueCardRequest body, IRequestContext request, ICommandHandler<IssuePlayerCardCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new IssuePlayerCardCommand(request.TenantId, new PlayerId(playerId), body.CardNumber, audit), ct)).ToHttp()))
            .RequirePermission("player.profile:issue_card")
            .ProducesNoContentCommand();

        api.MapPost("/{playerId:guid}/cards/cancel", async (Guid playerId, PlayerReasonRequest body, IRequestContext request, ICommandHandler<CancelPlayerCardCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new CancelPlayerCardCommand(request.TenantId, new PlayerId(playerId), audit), ct)).ToHttp()))
            .RequirePermission("player.profile:cancel_card")
            .ProducesNoContentCommand();

        api.MapPost("/{playerId:guid}/restrictions", async (Guid playerId, AddRestrictionRequest body, IRequestContext request, ICommandHandler<AddPlayerRestrictionCommand, Guid> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new AddPlayerRestrictionCommand(request.TenantId, new PlayerId(playerId), body.Type, body.Note, body.Until, audit), ct))
                .ToHttp(id => Results.Created($"/api/v1/players/{playerId}/restrictions/{id}", new CreatedId(id)))))
            .RequirePermission("player.restrictions:add")
            .ProducesCreatedCommand();

        api.MapPost("/{playerId:guid}/restrictions/{restrictionId:guid}/remove", async (Guid playerId, Guid restrictionId, PlayerReasonRequest body, IRequestContext request, ICommandHandler<RemovePlayerRestrictionCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new RemovePlayerRestrictionCommand(request.TenantId, new PlayerId(playerId), restrictionId, audit), ct)).ToHttp()))
            .RequirePermission("player.restrictions:remove")
            .ProducesNoContentCommand();

        return app;
    }

    private static async Task<IResult> WithReason(string? reasonCode, IRequestContext request, Func<AuditContext, Task<IResult>> run)
    {
        var reason = ReasonCode.Create(reasonCode);
        return reason.IsSuccess ? await run(request.Audit(reason.Value)) : ResultHttpExtensions.Problem(reason.Error);
    }
}
