using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Contracts;

/// <summary>Oyuncunun şu an oynadığı yer (Spec 007 KK7).</summary>
public sealed record PlayerLocationDto(Guid PlayerId, string TableCode, DateTimeOffset Since);

public interface IPlayerLocationQuery
{
    /// <summary>Süren rating yoksa null.</summary>
    Task<PlayerLocationDto?> CurrentLocation(TenantId tenantId, Guid playerId, CancellationToken ct);
}

/// <summary>Masa veya rating üzerinde işlem (denetim).</summary>
public sealed record FloorChangedV1(
    Guid EventId, Guid CorrelationId, DateTimeOffset OccurredAt, TenantId TenantId, AuditContext Audit, string Kind, string Detail) : IAuditableEvent
{
    public string Summary => $"Canlı oyun: {Kind} {Detail}".TrimEnd();
}

/// <summary>Rating bitti veya düzeltildi; loyalty puanı bu olaydan hesaplanır (KK8). Düzeltmede değerler günceldir.</summary>
public sealed record PlayerRatedV1(
    Guid EventId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    TenantId TenantId,
    AuditContext Audit,
    Guid RatingId,
    Guid PlayerId,
    string TableCode,
    string Currency,
    decimal AverageBet,
    decimal? PlayerResult,
    decimal TheoreticalWin,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    bool IsCorrection) : IAuditableEvent
{
    public string Summary => $"Rating {(IsCorrection ? "düzeltildi" : "bitti")}: {TableCode} ort. bahis {AverageBet} {Currency}, teorik {TheoreticalWin}";
}
