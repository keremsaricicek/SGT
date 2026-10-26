using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.Modules.Reporting.Application.Daily;

namespace Sgt.Modules.Reporting.Infrastructure;

/// <summary>Raporlama kendi verisini tutmaz; diğer modüllerin sözleşmelerini birleştirir.</summary>
public static class ReportingInfrastructureModule
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services) =>
        services.AddScoped<IQueryHandler<GetDailySummaryQuery, DailySummary>, GetDailySummaryHandler>();
}
