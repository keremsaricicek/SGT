using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Configuration.Application.ExchangeRates;
using Sgt.Modules.Configuration.Contracts;

namespace Sgt.Modules.Configuration.Api;

public sealed record SetExchangeRateRequest(string Currency, decimal Buy, decimal Sell, string ReasonCode);

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/configuration").RequireAuthorization(SgtClaims.Policy).WithTags("Configuration");

        api.MapGet("/exchange-rates/current", async (IRequestContext request, IQueryHandler<GetCurrentRatesQuery, IReadOnlyList<ExchangeRateDto>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new GetCurrentRatesQuery(request.TenantId), ct)));

        api.MapPost("/exchange-rates", async (SetExchangeRateRequest body, IRequestContext request, ICommandHandler<SetExchangeRateCommand> handler, CancellationToken ct) =>
        {
            var reason = ReasonCode.Create(body.ReasonCode);
            if (reason.IsFailure)
            {
                return ResultHttpExtensions.Problem(reason.Error);
            }

            var result = await handler.Handle(
                new SetExchangeRateCommand(request.TenantId, body.Currency, body.Buy, body.Sell, request.Audit(reason.Value)), ct);
            return result.ToHttp();
        }).RequirePermission("admin.settings:exchange_rate");

        return app;
    }
}
