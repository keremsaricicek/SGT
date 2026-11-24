using System.Diagnostics.CodeAnalysis;

namespace Sgt.BuildingBlocks;

/// <summary>
/// Modüller arası olay. Adı geçmiş zamandadır ve sürüm taşır (ör. PermissionChangedV1); immutable record'dur.
/// Tip adı kalıcıdır, yeniden adlandırılmaz (ADR-0004).
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>İdempotency anahtarı.</summary>
    Guid EventId { get; }

    Guid CorrelationId { get; }

    DateTimeOffset OccurredAt { get; }

    TenantId TenantId { get; }
}

/// <summary>Denetim kaydına düşmesi gereken olay (Spec 002 KK6).</summary>
public interface IAuditableEvent : IIntegrationEvent
{
    AuditContext Audit { get; }

    /// <summary>İnsanın okuyacağı kısa açıklama. Hassas veri içermez.</summary>
    string Summary { get; }
}

/// <summary>Bir olayı işleyen tüketici. İdempotent olmalıdır (inbox).</summary>
[SuppressMessage("Naming", "CA1711", Justification = "Proje sözlüğündeki ad; .NET event delegesi değildir.")]
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task Handle(TEvent integrationEvent, CancellationToken ct);
}

/// <summary>Tüketicinin modülündeki işlenmiş olay kaydı. İşlem ve kayıt aynı transaction'da yazılır.</summary>
public interface IInbox
{
    Task<bool> AlreadyProcessed(Guid eventId, string handler, CancellationToken ct);

    void MarkProcessed(Guid eventId, string handler);
}

/// <summary>Bir modülün outbox'ı üzerinde operatör işlemleri (Spec 002 KK5).</summary>
public interface IOutboxStore
{
    string Module { get; }

    Task<IReadOnlyList<DeadLetter>> DeadLetters(TenantId tenantId, int take, CancellationToken ct);

    Task<bool> Reprocess(TenantId tenantId, Guid eventId, CancellationToken ct);
}

public sealed record DeadLetter(string Module, Guid EventId, string Type, int Attempts, string? LastError, DateTimeOffset OccurredAt);
