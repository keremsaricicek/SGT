using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Reporting.Application.Daily;

namespace Sgt.Modules.Reporting.Api;

public static class ReportingEndpoints
{
    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/reports").RequireAuthorization(SgtClaims.Policy).WithTags("Reports")
            .MapGet("/daily", async (DateOnly? date, IRequestContext request, IQueryHandler<GetDailySummaryQuery, DailySummary> handler, CancellationToken ct) =>
                Results.Ok(await handler.Handle(new GetDailySummaryQuery(request.TenantId, date), ct)))
            .RequirePermission("report.daily:view");
        return app;
    }
}
