namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Gönderilmeyi bekleyen olay. İş verisiyle aynı transaction'da yazılır (Spec 002 KK1).</summary>
public sealed class OutboxMessage
{
    public const int MaxAttempts = 10;
    private static readonly TimeSpan MaxDelay = TimeSpan.FromHours(1);

    public Guid Id { get; private set; }

    /// <summary>Operatör ekranının tenant kapsamı için.</summary>
    public Guid TenantId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public bool DeadLettered { get; private set; }

    public static OutboxMessage Create(Guid eventId, Guid tenantId, string type, string payload, DateTimeOffset occurredAt) => new()
    {
        Id = eventId,
        TenantId = tenantId,
        Type = type,
        Payload = payload,
        OccurredAt = occurredAt,
        NextAttemptAt = occurredAt,
    };

    public void MarkProcessed(DateTimeOffset at) => ProcessedAt = at;

    /// <summary>Üstel bekleme: 2^deneme saniye, en çok 1 saat; 10. denemede dead-letter (KK4).</summary>
    public void MarkFailed(string error, DateTimeOffset at)
    {
        Attempts++;
        LastError = error.Length > 2000 ? error[..2000] : error;
        if (Attempts >= MaxAttempts)
        {
            DeadLettered = true;
            return;
        }

        var delay = TimeSpan.FromSeconds(Math.Pow(2, Attempts));
        NextAttemptAt = at + (delay < MaxDelay ? delay : MaxDelay);
    }

    /// <summary>Operatör yeniden işletir (KK5).</summary>
    public void Reprocess(DateTimeOffset at)
    {
        DeadLettered = false;
        Attempts = 0;
        NextAttemptAt = at;
    }
}

/// <summary>Bir tüketicinin işlediği olay; ikinci işlemi engeller (KK3).</summary>
public sealed class InboxMessage
{
    public Guid EventId { get; init; }

    public string Handler { get; init; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; init; }
}
