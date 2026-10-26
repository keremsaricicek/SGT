using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Domain;

/// <summary>Casino oyuncusu: kişisel bilgiler, kartlar ve kısıtlamalar.</summary>
public sealed class Player
{
    private readonly List<PlayerCard> _cards = [];
    private readonly List<PlayerRestriction> _restrictions = [];
    private readonly List<PlayerChange> _pending = [];

    private Player(PlayerId id, TenantId tenantId, PlayerNumber number, PlayerDetails details)
    {
        Id = id;
        TenantId = tenantId;
        Number = number;
        Details = details;
    }

    public PlayerId Id { get; }

    public TenantId TenantId { get; }

    public PlayerNumber Number { get; }

    public PlayerDetails Details { get; private set; }

    public IReadOnlyList<PlayerCard> Cards => _cards;

    public PlayerCard? ActiveCard => _cards.SingleOrDefault(c => c.IsActive);

    public IReadOnlyList<PlayerRestriction> Restrictions => _restrictions;

    public IReadOnlyList<PlayerChange> PendingChanges => _pending;

    public static Result<Player> Register(
        PlayerId id, TenantId tenantId, PlayerNumber number, PlayerDetails details, DateOnly today, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(details);
        var normalized = details.Normalize(today);
        if (normalized.IsFailure)
        {
            return normalized.Error;
        }

        var player = new Player(id, tenantId, number, normalized.Value);
        player.Record(PlayerChangeKind.Registered, string.Empty, audit, at);
        return player;
    }

    public static Player Restore(
        PlayerId id, TenantId tenantId, PlayerNumber number, PlayerDetails details,
        IEnumerable<PlayerCard> cards, IEnumerable<PlayerRestriction> restrictions)
    {
        var player = new Player(id, tenantId, number, details);
        player._cards.AddRange(cards);
        player._restrictions.AddRange(restrictions);
        return player;
    }

    public Result UpdateDetails(PlayerDetails details, DateOnly today, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(details);
        var normalized = details.Normalize(today);
        if (normalized.IsFailure)
        {
            return normalized.Error;
        }

        Details = normalized.Value;
        Record(PlayerChangeKind.DetailsUpdated, string.Empty, audit, at);
        return Result.Success();
    }

    /// <summary>Yeni kart verir; varsa önceki aktif kart iptal edilir (KK7).</summary>
    public void IssueCard(CardNumber number, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(number);
        ActiveCard?.Cancel(at);
        _cards.Add(new PlayerCard(number, at));
        Record(PlayerChangeKind.CardIssued, number.Value, audit, at);
    }

    public Result CancelCard(AuditContext audit, DateTimeOffset at)
    {
        var card = ActiveCard;
        if (card is null)
        {
            return PlayerErrors.NoActiveCard;
        }

        card.Cancel(at);
        Record(PlayerChangeKind.CardCancelled, card.Number.Value, audit, at);
        return Result.Success();
    }

    public Result<Guid> AddRestriction(RestrictionType type, string? note, DateTimeOffset? until, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (until is { } end && end <= at)
        {
            return PlayerErrors.RestrictionEndInPast;
        }

        var restriction = new PlayerRestriction(Guid.NewGuid(), type, string.IsNullOrWhiteSpace(note) ? null : note.Trim(), until, audit.Actor, at);
        _restrictions.Add(restriction);
        Record(PlayerChangeKind.RestrictionAdded, type.ToString(), audit, at);
        return restriction.Id;
    }

    /// <summary>Dört göz: ekleyen kişi kaldıramaz (KK9).</summary>
    public Result RemoveRestriction(Guid restrictionId, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var restriction = _restrictions.SingleOrDefault(r => r.Id == restrictionId && r.IsActiveAt(at));
        if (restriction is null)
        {
            return PlayerErrors.RestrictionNotFound;
        }

        if (restriction.AddedBy == audit.Actor)
        {
            return PlayerErrors.FourEyesRequired;
        }

        restriction.Remove(audit.Actor, at);
        Record(PlayerChangeKind.RestrictionRemoved, restriction.Type.ToString(), audit, at);
        return Result.Success();
    }

    public bool IsRestricted(DateTimeOffset at) => _restrictions.Any(r => r.IsActiveAt(at));

    public void ClearPendingChanges() => _pending.Clear();

    private void Record(PlayerChangeKind kind, string detail, AuditContext audit, DateTimeOffset at) =>
        _pending.Add(new PlayerChange(Guid.NewGuid(), kind, detail, audit, at));
}
