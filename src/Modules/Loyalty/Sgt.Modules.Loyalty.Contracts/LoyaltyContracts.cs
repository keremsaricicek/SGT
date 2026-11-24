using Sgt.BuildingBlocks;

namespace Sgt.Modules.Loyalty.Contracts;

/// <summary>Oyuncu seviyesi (Bronze, Silver, Gold, Platinum) — Spec 008 KK7.</summary>
public interface IPlayerTierQuery
{
    Task<string> GetTier(TenantId tenantId, Guid playerId, CancellationToken ct);
}

/// <summary>Loyalty işlemi (puan, istek, karar, kural) — denetim.</summary>
public sealed record LoyaltyChangedV1(
    Guid EventId, Guid CorrelationId, DateTimeOffset OccurredAt, TenantId TenantId, AuditContext Audit, Guid? PlayerId, string Kind, string Detail)
    : IAuditableEvent
{
    public string Summary => $"Loyalty: {Kind} {Detail}".TrimEnd();
}

/// <summary>Günlük puan toplamı (Spec 011 KK5).</summary>
public sealed record DailyLoyaltySummary(long PointsEarned, long PointsRedeemed);

public interface IDailyLoyaltyQuery
{
    Task<DailyLoyaltySummary> For(TenantId tenantId, BusinessDay day, CancellationToken ct);
}
