using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Finance.Application.Cage;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Contracts;
using Sgt.Modules.Finance.Infrastructure.Persistence;

namespace Sgt.Modules.Finance.Infrastructure;

public static class FinanceInfrastructureModule
{
    public static IServiceCollection AddFinanceModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FinanceDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", FinanceDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddOutbox<FinanceDbContext>("Finance");
        services.AddIntegrationEventType<CageShiftChangedV1>();

        services.AddScoped<TrackedShifts>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IFinanceUnitOfWork, FinanceUnitOfWork>();
        services.AddScoped<CageOperation>();

        services.AddScoped<ICommandHandler<OpenShiftCommand, Guid>, OpenShiftHandler>();
        services.AddScoped<ICommandHandler<CloseShiftCommand>, CloseShiftHandler>();
        services.AddScoped<SellChipsHandler>();
        services.AddScoped<RedeemChipsHandler>();
        services.AddScoped<ExchangeForeignToBaseHandler>();
        services.AddScoped<ExchangeBaseToForeignHandler>();
        services.AddScoped<IQueryHandler<GetShiftSummaryQuery, ShiftSummary?>, GetShiftSummaryHandler>();
        return services;
    }

    public static Task MigrateFinanceDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<FinanceDbContext>().Database.MigrateAsync(ct);
    }
}
