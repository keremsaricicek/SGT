using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Domain;

public enum PlayerChangeKind
{
    Registered,
    DetailsUpdated,
    CardIssued,
    CardCancelled,
    RestrictionAdded,
    RestrictionRemoved,
}

/// <summary>Oyuncu üzerindeki bir işlem; denetime düşer. Detail kişisel veri içermez (KK11).</summary>
public sealed record PlayerChange(Guid Id, PlayerChangeKind Kind, string Detail, AuditContext Audit, DateTimeOffset OccurredAt);
