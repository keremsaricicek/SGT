using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.Loyalty.Application.Adjustments;
using Sgt.Modules.Loyalty.Application.Comps;
using Sgt.Modules.Loyalty.Application.Earning;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Application.Rules;
using Sgt.Modules.Loyalty.Application.Summary;
using Sgt.Modules.Loyalty.Contracts;
using Sgt.Modules.Loyalty.Infrastructure.Persistence;

namespace Sgt.Modules.Loyalty.Infrastructure;

public static class LoyaltyInfrastructureModule
{
    public static IServiceCollection AddLoyaltyModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<LoyaltyDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", LoyaltyDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddOutbox<LoyaltyDbContext>("Loyalty");
        services.AddIntegrationEventType<LoyaltyChangedV1>();
        services.AddIntegrationEventType<PlayerRatedV1>();

        services.AddScoped<TrackedLoyalty>();
        services.AddScoped<ILoyaltyAccountRepository, LoyaltyAccountRepository>();
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
        services.AddScoped<IEarningRuleStore, EarningRuleStore>();
        services.AddScoped<ILoyaltyInbox, LoyaltyInbox>();
        services.AddScoped<ILoyaltyUnitOfWork, LoyaltyUnitOfWork>();
        services.AddScoped<IPlayerTierQuery, PlayerTierQuery>();
        services.AddScoped<IDailyLoyaltyQuery, DailyLoyaltyQuery>();

        services.AddScoped<IIntegrationEventHandler<PlayerRatedV1>, PlayerRatedHandler>();
        services.AddScoped<ICommandHandler<RequestCompCommand, Guid>, RequestCompHandler>();
        services.AddScoped<ICommandHandler<DecideCompCommand>, DecideCompHandler>();
        services.AddScoped<ICommandHandler<RequestPointsAdjustmentCommand, Guid>, RequestPointsAdjustmentHandler>();
        services.AddScoped<ICommandHandler<DecidePointsAdjustmentCommand>, DecidePointsAdjustmentHandler>();
        services.AddScoped<ICommandHandler<SetEarningRuleCommand>, SetEarningRuleHandler>();
        services.AddScoped<IQueryHandler<GetLoyaltySummaryQuery, LoyaltySummary>, GetLoyaltySummaryHandler>();
        return services;
    }

    public static Task MigrateLoyaltyDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<LoyaltyDbContext>().Database.MigrateAsync(ct);
    }
}
