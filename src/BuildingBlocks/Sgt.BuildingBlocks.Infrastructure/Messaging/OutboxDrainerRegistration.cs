using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Anında outbox boşaltıcılarının kaydı. Yalnızca demo verisi üretimi (Sgt.DemoData) ve testler çağırır; üretim API'si
/// olayları arka plan servisiyle dağıtır (mimari testi denetler).
/// </summary>
public static class OutboxDrainerRegistration
{
    /// <summary>Kayıtlı her outbox için bir boşaltıcı açar; modül kayıtlarından önce veya sonra çağrılabilir.</summary>
    public static IServiceCollection AddOutboxDrainers(this IServiceCollection services)
    {
        services.TryAddSingleton(sp => new OutboxDrainers(sp.GetServices<OutboxRegistration>().Select(r => (IOutboxDrainer)ActivatorUtilities.CreateInstance(
            sp, typeof(OutboxDrainer<>).MakeGenericType(r.ContextType), r.Module))));
        return services;
    }
}
