using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Configuration.Application.ExchangeRates;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Configuration.Infrastructure.Persistence;

namespace Sgt.Modules.Configuration.Infrastructure;

public static class ConfigurationInfrastructureModule
{
    public static IServiceCollection AddConfigurationModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ConfigurationDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", ConfigurationDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddOutbox<ConfigurationDbContext>("Configuration");
        services.AddIntegrationEventType<ExchangeRateSetV1>();

        services.AddScoped<IExchangeRateStore, ExchangeRateStore>();
        services.AddScoped<IConfigurationOutbox, ConfigurationOutbox>();
        services.AddScoped<IConfigurationUnitOfWork, ConfigurationUnitOfWork>();
        services.AddScoped<IExchangeRateQuery, ExchangeRateQuery>();

        services.AddScoped<ICommandHandler<SetExchangeRateCommand>, SetExchangeRateHandler>();
        services.AddScoped<IQueryHandler<GetCurrentRatesQuery, IReadOnlyList<ExchangeRateDto>>, GetCurrentRatesHandler>();
        return services;
    }

    public static Task MigrateConfigurationDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<ConfigurationDbContext>().Database.MigrateAsync(ct);
    }
}
