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
        services.AddSingleton(new OutboxRegistration(module, typeof(TContext)));
        services.AddScoped<IOutboxStore>(sp =>
            new OutboxStore<TContext>(module, sp.GetRequiredService<TContext>(), sp.GetRequiredService<IClock>()));
        return services;
    }

    /// <summary>
    /// Kayıtlı her outbox için anında boşaltıcı açar (<see cref="OutboxDrainers"/>). Yalnızca demo verisi üretimi ve testler içindir;
    /// üretim API'si olayları arka plan servisiyle dağıtır. Modül kayıtlarından önce veya sonra çağrılabilir.
    /// </summary>
    public static IServiceCollection AddOutboxDrainers(this IServiceCollection services)
    {
        services.TryAddSingleton(sp => new OutboxDrainers(sp.GetServices<OutboxRegistration>().Select(r => (IOutboxDrainer)ActivatorUtilities.CreateInstance(
            sp, typeof(OutboxDrainer<>).MakeGenericType(r.ContextType), r.Module))));
        return services;
    }
}

/// <summary>Bir modülün outbox kaydı (modül adı ve DbContext tipi); boşaltıcıların kurulması için.</summary>
public sealed record OutboxRegistration(string Module, Type ContextType);
