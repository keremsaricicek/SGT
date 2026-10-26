using System.Text.Json;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Kayıtlı bir olay tipi. Modüller yayınladıkları her olayı kaydeder.</summary>
public sealed record IntegrationEventType(Type Type)
{
    public string Name => Type.FullName!;
}

/// <summary>Olayları outbox için JSON'a çevirir ve geri okur. Yalnızca kayıtlı tipler kabul edilir.</summary>
public sealed class IntegrationEventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, Type> _types;

    public IntegrationEventSerializer(IEnumerable<IntegrationEventType> types) =>
        _types = types.DistinctBy(t => t.Name).ToDictionary(t => t.Name, t => t.Type);

    public OutboxMessage ToOutbox(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        var type = integrationEvent.GetType();
        if (!_types.ContainsKey(type.FullName!))
        {
            throw new InvalidOperationException($"Olay tipi kayıtlı değil: {type.FullName}");
        }

        return OutboxMessage.Create(
            integrationEvent.EventId,
            integrationEvent.TenantId.Value,
            type.FullName!,
            JsonSerializer.Serialize(integrationEvent, type, Options),
            integrationEvent.OccurredAt);
    }

    public IIntegrationEvent FromOutbox(OutboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!_types.TryGetValue(message.Type, out var type))
        {
            throw new InvalidOperationException($"Olay tipi kayıtlı değil: {message.Type}");
        }

        return (IIntegrationEvent)(JsonSerializer.Deserialize(message.Payload, type, Options)
            ?? throw new InvalidOperationException($"Olay okunamadı: {message.Id}"));
    }
}
