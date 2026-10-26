using Sgt.BuildingBlocks;

namespace Sgt.Modules.Loyalty.Domain;

public enum PointsEntryKind
{
    Earned,
    Adjusted,
    Redeemed,
}

/// <summary>Silinmez puan hareketi. SourceId: rating, düzeltme isteği veya comp isteği.</summary>
public sealed record PointsEntry(Guid Id, PointsEntryKind Kind, long Points, Guid SourceId, AuditContext Audit, DateTimeOffset At);

public enum LoyaltyChangeKind
{
    PointsEarned,
    PointsAdjusted,
    PointsRedeemed,
    AdjustmentRequested,
    CompRequested,
    RequestApproved,
    RequestRejected,
    RuleChanged,
}

public sealed record LoyaltyChange(Guid Id, Guid PlayerId, LoyaltyChangeKind Kind, string Detail, AuditContext Audit, DateTimeOffset At);

/// <summary>Oyuncunun puan hesabı. Bakiye hareketlerin toplamıdır ve eksiye düşmez (Spec 008 KK3).</summary>
public sealed class LoyaltyAccount
{
    private readonly List<PointsEntry> _entries = [];
    private readonly List<LoyaltyChange> _pending = [];

    private LoyaltyAccount(TenantId tenantId, Guid playerId)
    {
        TenantId = tenantId;
        PlayerId = playerId;
    }

    public TenantId TenantId { get; }

    public Guid PlayerId { get; }

    public IReadOnlyList<PointsEntry> Entries => _entries;

    public long Balance => _entries.Sum(e => e.Points);

    public long LifetimeEarned => _entries.Where(e => e.Kind == PointsEntryKind.Earned).Sum(e => e.Points);

    public Tier Tier => Tiers.For(LifetimeEarned);

    public IReadOnlyList<LoyaltyChange> PendingChanges => _pending;

    public static LoyaltyAccount Open(TenantId tenantId, Guid playerId) => new(tenantId, playerId);

    public static LoyaltyAccount Restore(TenantId tenantId, Guid playerId, IEnumerable<PointsEntry> entries)
    {
        var account = new LoyaltyAccount(tenantId, playerId);
        account._entries.AddRange(entries);
        return account;
    }

    /// <summary>
    /// Rating puanını uygular; düzeltmede yalnızca fark kaydedilir (KK2). Puan harcanmışsa düşüş bakiyeyi
    /// sıfırın altına indirmez.
    /// </summary>
    public void EarnFromRating(Guid ratingId, long points, AuditContext audit, DateTimeOffset at)
    {
        var alreadyEarned = _entries.Where(e => e.Kind == PointsEntryKind.Earned && e.SourceId == ratingId).Sum(e => e.Points);
        var delta = Math.Max(points - alreadyEarned, -Balance);
        if (delta == 0)
        {
            return;
        }

        Add(PointsEntryKind.Earned, delta, ratingId, audit, at, LoyaltyChangeKind.PointsEarned);
    }

    public Result Adjust(Guid requestId, long points, AuditContext audit, DateTimeOffset at)
    {
        if (points == 0)
        {
            return LoyaltyErrors.InvalidPoints;
        }

        if (Balance + points < 0)
        {
            return LoyaltyErrors.InsufficientPoints;
        }

        Add(PointsEntryKind.Adjusted, points, requestId, audit, at, LoyaltyChangeKind.PointsAdjusted);
        return Result.Success();
    }

    public Result Redeem(Guid compId, long points, AuditContext audit, DateTimeOffset at)
    {
        if (points <= 0)
        {
            return LoyaltyErrors.InvalidPoints;
        }

        if (Balance < points)
        {
            return LoyaltyErrors.InsufficientPoints;
        }

        Add(PointsEntryKind.Redeemed, -points, compId, audit, at, LoyaltyChangeKind.PointsRedeemed);
        return Result.Success();
    }

    public void ClearPendingChanges() => _pending.Clear();

    private void Add(PointsEntryKind kind, long points, Guid sourceId, AuditContext audit, DateTimeOffset at, LoyaltyChangeKind change)
    {
        _entries.Add(new PointsEntry(Guid.NewGuid(), kind, points, sourceId, audit, at));
        _pending.Add(new LoyaltyChange(Guid.NewGuid(), PlayerId, change, $"{points} puan", audit, at));
    }
}
