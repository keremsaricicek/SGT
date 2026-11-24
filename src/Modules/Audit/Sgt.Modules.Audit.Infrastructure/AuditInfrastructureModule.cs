using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Audit.Application.Ports;
using Sgt.Modules.Audit.Application.Recording;
using Sgt.Modules.Audit.Application.Search;
using Sgt.Modules.Audit.Infrastructure.Persistence;

namespace Sgt.Modules.Audit.Infrastructure;

public static class AuditInfrastructureModule
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuditDbContext>(o => o
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", AuditDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddIntegrationEvents();
        services.AddScoped<IAuditLog, AuditLog>();
        services.AddScoped<IAuditSearch, AuditSearch>();
        services.AddScoped<IAuditInbox, AuditInbox>();
        services.AddScoped<IAuditUnitOfWork, AuditUnitOfWork>();

        services.AddScoped<IIntegrationEventHandler<IAuditableEvent>, RecordAuditEntryHandler>();
        services.AddScoped<IQueryHandler<SearchAuditEntriesQuery, IReadOnlyList<AuditEntryView>>, SearchAuditEntriesHandler>();
        return services;
    }

    public static Task MigrateAuditDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<AuditDbContext>().Database.MigrateAsync(ct);
    }
}
