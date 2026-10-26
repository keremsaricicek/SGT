using Sgt.BuildingBlocks;

namespace Sgt.Modules.Loyalty.Domain;

public enum DecisionStatus
{
    Pending,
    Approved,
    Rejected,
}

/// <summary>Dört göz kararı: isteyen kişi karar veremez (Spec 008 KK5–KK6).</summary>
public sealed class Decision
{
    internal Decision(UserId requestedBy, DecisionStatus status = DecisionStatus.Pending, UserId? decidedBy = null, DateTimeOffset? decidedAt = null)
    {
        RequestedBy = requestedBy;
        Status = status;
        DecidedBy = decidedBy;
        DecidedAt = decidedAt;
    }

    public UserId RequestedBy { get; }

    public DecisionStatus Status { get; private set; }

    public UserId? DecidedBy { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }

    public static Decision Restore(UserId requestedBy, DecisionStatus status, UserId? decidedBy, DateTimeOffset? decidedAt) =>
        new(requestedBy, status, decidedBy, decidedAt);

    internal Result Decide(UserId by, bool approve, DateTimeOffset at)
    {
        if (Status != DecisionStatus.Pending)
        {
            return LoyaltyErrors.AlreadyDecided;
        }

        if (by == RequestedBy)
        {
            return LoyaltyErrors.FourEyesRequired;
        }

        Status = approve ? DecisionStatus.Approved : DecisionStatus.Rejected;
        DecidedBy = by;
        DecidedAt = at;
        return Result.Success();
    }
}

/// <summary>Puanla ikram isteği (KK6).</summary>
public sealed class CompRequest
{
    private readonly List<LoyaltyChange> _pending = [];

    private CompRequest(Guid id, TenantId tenantId, Guid playerId, long points, string description, DateTimeOffset requestedAt, Decision decision)
    {
        Id = id;
        TenantId = tenantId;
        PlayerId = playerId;
        Points = points;
        Description = description;
        RequestedAt = requestedAt;
        Decision = decision;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public Guid PlayerId { get; }

    public long Points { get; }

    public string Description { get; }

    public DateTimeOffset RequestedAt { get; }

    public Decision Decision { get; }

    public IReadOnlyList<LoyaltyChange> PendingChanges => _pending;

    public static Result<CompRequest> Create(TenantId tenantId, Guid playerId, long points, string description, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (points <= 0)
        {
            return LoyaltyErrors.InvalidPoints;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return LoyaltyErrors.DescriptionRequired;
        }

        var request = new CompRequest(Guid.NewGuid(), tenantId, playerId, points, description.Trim(), at, new Decision(audit.Actor));
        request._pending.Add(new LoyaltyChange(Guid.NewGuid(), playerId, LoyaltyChangeKind.CompRequested, $"{points} puan", audit, at));
        return request;
    }

    public static CompRequest Restore(Guid id, TenantId tenantId, Guid playerId, long points, string description, DateTimeOffset requestedAt, Decision decision) =>
        new(id, tenantId, playerId, points, description, requestedAt, decision);

    /// <summary>Onayda puan hesaptan düşülür; bakiye yetmezse karar verilmez.</summary>
    public Result Decide(LoyaltyAccount? account, bool approve, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var decided = Decision.Decide(audit.Actor, approve, at);
        if (decided.IsFailure || !approve)
        {
            RecordDecision(decided, approve, audit, at);
            return decided;
        }

        var redeemed = account?.Redeem(Id, Points, audit, at) ?? LoyaltyErrors.InsufficientPoints;
        RecordDecision(redeemed, approve, audit, at);
        return redeemed;
    }

    public void ClearPendingChanges() => _pending.Clear();

    private void RecordDecision(Result result, bool approve, AuditContext audit, DateTimeOffset at)
    {
        if (result.IsSuccess)
        {
            _pending.Add(new LoyaltyChange(Guid.NewGuid(), PlayerId, approve ? LoyaltyChangeKind.RequestApproved : LoyaltyChangeKind.RequestRejected, $"comp {Points} puan", audit, at));
        }
    }
}

/// <summary>Elle puan düzeltme isteği (KK5).</summary>
public sealed class PointsAdjustmentRequest
{
    private readonly List<LoyaltyChange> _pending = [];

    private PointsAdjustmentRequest(Guid id, TenantId tenantId, Guid playerId, long points, string note, DateTimeOffset requestedAt, Decision decision)
    {
        Id = id;
        TenantId = tenantId;
        PlayerId = playerId;
        Points = points;
        Note = note;
        RequestedAt = requestedAt;
        Decision = decision;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public Guid PlayerId { get; }

    public long Points { get; }

    public string Note { get; }

    public DateTimeOffset RequestedAt { get; }

    public Decision Decision { get; }

    public IReadOnlyList<LoyaltyChange> PendingChanges => _pending;

    public static Result<PointsAdjustmentRequest> Create(TenantId tenantId, Guid playerId, long points, string note, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (points == 0)
        {
            return LoyaltyErrors.InvalidPoints;
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            return LoyaltyErrors.DescriptionRequired;
        }

        var request = new PointsAdjustmentRequest(Guid.NewGuid(), tenantId, playerId, points, note.Trim(), at, new Decision(audit.Actor));
        request._pending.Add(new LoyaltyChange(Guid.NewGuid(), playerId, LoyaltyChangeKind.AdjustmentRequested, $"{points} puan", audit, at));
        return request;
    }

    public static PointsAdjustmentRequest Restore(
        Guid id, TenantId tenantId, Guid playerId, long points, string note, DateTimeOffset requestedAt, Decision decision) =>
        new(id, tenantId, playerId, points, note, requestedAt, decision);

    public Result Decide(LoyaltyAccount account, bool approve, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(audit);
        var decided = Decision.Decide(audit.Actor, approve, at);
        var result = decided.IsFailure || !approve ? decided : account.Adjust(Id, Points, audit, at);
        if (result.IsSuccess)
        {
            _pending.Add(new LoyaltyChange(Guid.NewGuid(), PlayerId, approve ? LoyaltyChangeKind.RequestApproved : LoyaltyChangeKind.RequestRejected, $"düzeltme {Points} puan", audit, at));
        }

        return result;
    }

    public void ClearPendingChanges() => _pending.Clear();
}

/// <summary>Puan kazanma kuralı: kaç TRY teorik kazanç = 1 puan (KK1).</summary>
public sealed class EarningRule
{
    public const decimal DefaultTryPerPoint = 100m;

    private EarningRule(TenantId tenantId, decimal tryPerPoint)
    {
        TenantId = tenantId;
        TryPerPoint = tryPerPoint;
    }

    public TenantId TenantId { get; }

    public decimal TryPerPoint { get; }

    public static Result<EarningRule> Set(TenantId tenantId, decimal tryPerPoint) =>
        tryPerPoint > 0 && decimal.Round(tryPerPoint, 2) == tryPerPoint ? new EarningRule(tenantId, tryPerPoint) : LoyaltyErrors.InvalidRule;

    public static EarningRule Restore(TenantId tenantId, decimal tryPerPoint) => new(tenantId, tryPerPoint);

    public long PointsFor(decimal theoreticalWinTry) =>
        theoreticalWinTry <= 0 ? 0 : (long)decimal.Floor(theoreticalWinTry / TryPerPoint);
}
