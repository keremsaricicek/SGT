using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Application.Ratings;
using Sgt.Modules.GamingFloor.Application.Tables;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.GamingFloor.Infrastructure.Persistence;

namespace Sgt.Modules.GamingFloor.Infrastructure;

public static class GamingFloorInfrastructureModule
{
    public static IServiceCollection AddGamingFloorModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FloorDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", FloorDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddOutbox<FloorDbContext>("GamingFloor");
        services.AddIntegrationEventType<FloorChangedV1>();
        services.AddIntegrationEventType<PlayerRatedV1>();

        services.AddScoped<TrackedFloor>();
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IFloorUnitOfWork, FloorUnitOfWork>();
        services.AddScoped<IPlayerLocationQuery, PlayerLocationQuery>();

        services.AddScoped<ICommandHandler<DefineTableCommand, Guid>, DefineTableHandler>();
        services.AddScoped<OpenTableSessionHandler>();
        services.AddScoped<CloseTableSessionHandler>();
        services.AddScoped<ICommandHandler<StartRatingCommand, Guid>, StartRatingHandler>();
        services.AddScoped<ICommandHandler<FinishRatingCommand>, FinishRatingHandler>();
        services.AddScoped<ICommandHandler<CorrectRatingCommand>, CorrectRatingHandler>();
        services.AddScoped<IQueryHandler<ListTablesQuery, IReadOnlyList<TableView>>, ListTablesHandler>();
        return services;
    }

    public static Task MigrateGamingFloorDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<FloorDbContext>().Database.MigrateAsync(ct);
    }
}
