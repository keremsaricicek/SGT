using Sgt.BuildingBlocks;

namespace Sgt.Modules.Loyalty.Domain;

/// <summary>Puanla ikram isteği (KK6).</summary>
public sealed class CompRequest
{
    private readonly List<LoyaltyChange> _pending = [];

    private CompRequest(Guid id, TenantId tenantId, Guid playerId, long points, string description, DateTimeOffset requestedAt, FourEyesDecision decision)
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

    public FourEyesDecision Decision { get; }

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

        var request = new CompRequest(Guid.NewGuid(), tenantId, playerId, points, description.Trim(), at, FourEyesDecision.Request(audit.Actor));
        request._pending.Add(new LoyaltyChange(Guid.NewGuid(), playerId, LoyaltyChangeKind.CompRequested, $"{points} puan", audit, at));
        return request;
    }

    public static CompRequest Restore(Guid id, TenantId tenantId, Guid playerId, long points, string description, DateTimeOffset requestedAt, FourEyesDecision decision) =>
        new(id, tenantId, playerId, points, description, requestedAt, decision);

    /// <summary>Onayda puan hesaptan düşülür; bakiye yetmezse karar verilmez.</summary>
    public Result Decide(LoyaltyAccount? account, bool approve, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var allowed = Decision.CanDecide(audit.Actor);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        // Onayda önce puan düşülür; bakiye yetmezse karar verilmez.
        var redeemed = approve ? account?.Redeem(Id, Points, audit, at) ?? LoyaltyErrors.InsufficientPoints : Result.Success();
        if (redeemed.IsFailure)
        {
            return redeemed;
        }

        Decision.Decide(audit.Actor, approve, at);
        RecordDecision(redeemed, approve, audit, at);
        return Result.Success();
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

    private PointsAdjustmentRequest(Guid id, TenantId tenantId, Guid playerId, long points, string note, DateTimeOffset requestedAt, FourEyesDecision decision)
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

    public FourEyesDecision Decision { get; }

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

        var request = new PointsAdjustmentRequest(Guid.NewGuid(), tenantId, playerId, points, note.Trim(), at, FourEyesDecision.Request(audit.Actor));
        request._pending.Add(new LoyaltyChange(Guid.NewGuid(), playerId, LoyaltyChangeKind.AdjustmentRequested, $"{points} puan", audit, at));
        return request;
    }

    public static PointsAdjustmentRequest Restore(
        Guid id, TenantId tenantId, Guid playerId, long points, string note, DateTimeOffset requestedAt, FourEyesDecision decision) =>
        new(id, tenantId, playerId, points, note, requestedAt, decision);

    public Result Decide(LoyaltyAccount account, bool approve, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(audit);
        var allowed = Decision.CanDecide(audit.Actor);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var result = approve ? account.Adjust(Id, Points, audit, at) : Result.Success();
        if (result.IsSuccess)
        {
            Decision.Decide(audit.Actor, approve, at);
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
