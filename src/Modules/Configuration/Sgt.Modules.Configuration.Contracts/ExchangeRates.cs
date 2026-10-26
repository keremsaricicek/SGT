using Sgt.BuildingBlocks;

namespace Sgt.Modules.Configuration.Contracts;

/// <summary>Bir an için geçerli kur. Buy/Sell: 1 birim yabancı paranın TRY karşılığı.</summary>
public sealed record ExchangeRateDto(string Currency, decimal Buy, decimal Sell, DateTimeOffset EffectiveFrom);

/// <summary>Diğer modüllerin kur okuması (Spec 004 KK6).</summary>
public interface IExchangeRateQuery
{
    /// <summary>O andan önce girilmiş en son kur; yoksa null (KK4).</summary>
    Task<ExchangeRateDto?> RateAt(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct);
}

/// <summary>Yeni kur girildi (denetime düşer).</summary>
public sealed record ExchangeRateSetV1(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    TenantId TenantId,
    AuditContext Audit,
    string Currency,
    decimal Buy,
    decimal Sell) : IAuditableEvent
{
    public string Summary => $"Kur {Currency}: alış {Buy}, satış {Sell}";
}
