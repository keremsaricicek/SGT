using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Olayı aynı süreçteki tüm tüketicilere iletir. Olayın kendi tipine ve uyguladığı olay arayüzlerine
/// (ör. IAuditableEvent) abone olan handler'lar çağrılır. Her handler kendi DI kapsamında çalışır.
/// </summary>
public sealed class IntegrationEventDispatcher(IServiceScopeFactory scopes)
{
    public async Task Dispatch(IIntegrationEvent integrationEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        var eventType = integrationEvent.GetType();
        var subscribedTypes = eventType.GetInterfaces()
            .Where(i => typeof(IIntegrationEvent).IsAssignableFrom(i))
            .Prepend(eventType);

        foreach (var handlerType in subscribedTypes.Select(t => typeof(IIntegrationEventHandler<>).MakeGenericType(t)))
        {
            var handle = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.Handle))!;
            await using var probe = scopes.CreateAsyncScope();
            var count = probe.ServiceProvider.GetServices(handlerType).Count();

            for (var i = 0; i < count; i++)
            {
                await using var scope = scopes.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetServices(handlerType).ElementAt(i)!;
                await Invoke(handle, handler, integrationEvent, ct);
            }
        }
    }

    private static async Task Invoke(MethodInfo handle, object handler, IIntegrationEvent integrationEvent, CancellationToken ct)
    {
        try
        {
            await (Task)handle.Invoke(handler, [integrationEvent, ct])!;
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
        }
    }
}
