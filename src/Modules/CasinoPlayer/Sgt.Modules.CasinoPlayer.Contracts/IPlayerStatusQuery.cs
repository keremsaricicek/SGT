using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Contracts;

/// <summary>Oyuncunun işlem görüp göremeyeceği (Spec 005 KK10). Kasa, masa ve loyalty bunu sorar.</summary>
public sealed record PlayerStatusDto(Guid PlayerId, string PlayerNumber, bool IsRestricted);

public interface IPlayerStatusQuery
{
    /// <summary>Oyuncu yoksa null.</summary>
    Task<PlayerStatusDto?> GetStatus(TenantId tenantId, Guid playerId, CancellationToken ct);
}

/// <summary>Oyuncu üzerinde bir işlem yapıldı. Özet yalnızca oyuncu numarası içerir (KK11).</summary>
public sealed record PlayerChangedV1(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    TenantId TenantId,
    AuditContext Audit,
    Guid PlayerId,
    string PlayerNumber,
    string Kind,
    string Detail) : IAuditableEvent
{
    public string Summary => $"Oyuncu {PlayerNumber}: {Kind} {Detail}".TrimEnd();
}
