using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>Ortak olay altyapısı. Birden çok kez çağrılabilir.</summary>
    public static IServiceCollection AddIntegrationEvents(this IServiceCollection services)
    {
        services.TryAddSingleton<IntegrationEventSerializer>();
        services.TryAddSingleton<IntegrationEventDispatcher>();
        return services;
    }

    public static IServiceCollection AddIntegrationEventType<TEvent>(this IServiceCollection services)
        where TEvent : IIntegrationEvent =>
        services.AddSingleton(new IntegrationEventType(typeof(TEvent)));

    /// <summary>Modülün outbox'ını, dağıtıcı servisini ve operatör erişimini kaydeder.</summary>
    public static IServiceCollection AddOutbox<TContext>(this IServiceCollection services, string module)
        where TContext : DbContext
    {
        services.AddIntegrationEvents();
        services.AddSingleton<OutboxProcessor<TContext>>();
        services.AddHostedService<OutboxDispatcherService<TContext>>();
        services.AddScoped<IOutboxStore>(sp =>
            new OutboxStore<TContext>(module, sp.GetRequiredService<TContext>(), sp.GetRequiredService<IClock>()));
        return services;
    }
}
