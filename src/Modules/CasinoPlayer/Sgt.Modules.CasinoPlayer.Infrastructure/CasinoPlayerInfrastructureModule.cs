using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.CasinoPlayer.Application.Cards;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.CasinoPlayer.Application.Restrictions;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.CasinoPlayer.Infrastructure.Persistence;

namespace Sgt.Modules.CasinoPlayer.Infrastructure;

/// <summary>CasinoPlayer kayıtları. Host, <see cref="IPersonalDataProtector"/> kaydını ayrıca yapar (ADR-0005).</summary>
public static class CasinoPlayerInfrastructureModule
{
    public static IServiceCollection AddCasinoPlayerModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PlayerDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", PlayerDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddOutbox<PlayerDbContext>("CasinoPlayer");
        services.AddIntegrationEventType<PlayerChangedV1>();

        services.AddScoped<PlayerMapping>();
        services.AddScoped<TrackedPlayers>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IPlayerUnitOfWork, PlayerUnitOfWork>();
        services.AddScoped<IPlayerStatusQuery, PlayerStatusQuery>();

        services.AddScoped<ICommandHandler<RegisterPlayerCommand, PlayerRegistered>, RegisterPlayerHandler>();
        services.AddScoped<ICommandHandler<UpdatePlayerDetailsCommand>, UpdatePlayerDetailsHandler>();
        services.AddScoped<ICommandHandler<IssuePlayerCardCommand>, IssuePlayerCardHandler>();
        services.AddScoped<ICommandHandler<CancelPlayerCardCommand>, CancelPlayerCardHandler>();
        services.AddScoped<ICommandHandler<AddPlayerRestrictionCommand, Guid>, AddPlayerRestrictionHandler>();
        services.AddScoped<ICommandHandler<RemovePlayerRestrictionCommand>, RemovePlayerRestrictionHandler>();
        services.AddScoped<IQueryHandler<GetPlayerQuery, PlayerView?>, GetPlayerHandler>();
        services.AddScoped<IQueryHandler<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>, SearchPlayersHandler>();
        return services;
    }

    public static Task MigrateCasinoPlayerDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<PlayerDbContext>().Database.MigrateAsync(ct);
    }
}
