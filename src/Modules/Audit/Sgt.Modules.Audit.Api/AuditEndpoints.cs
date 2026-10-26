using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Audit.Application.Search;

namespace Sgt.Modules.Audit.Api;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/audit").RequireAuthorization(SgtClaims.Policy).WithTags("Audit")
            .MapGet("/entries", async (
                DateTimeOffset? from,
                DateTimeOffset? to,
                Guid? actorId,
                Guid? correlationId,
                int? take,
                IRequestContext request,
                IQueryHandler<SearchAuditEntriesQuery, IReadOnlyList<AuditEntryView>> handler,
                CancellationToken ct) =>
                Results.Ok(await handler.Handle(
                    new SearchAuditEntriesQuery(
                        request.TenantId, from, to, actorId is { } a ? new UserId(a) : null, correlationId, take ?? 100),
                    ct)))
            .RequirePermission("admin.audit:view");

        return app;
    }
}
