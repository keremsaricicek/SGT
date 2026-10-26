namespace Sgt.BuildingBlocks;

public enum DecisionStatus
{
    Pending,
    Approved,
    Rejected,
}

/// <summary>
/// Dört göz (maker-checker) kararı: isteyen kişi karar veremez (anayasa III). Değer taşıyan isteklerde
/// kompozisyonla kullanılır.
/// </summary>
public sealed class FourEyesDecision
{
    public static readonly Error SameUser =
        new("Common.FourEyesRequired", "İsteği yapan kişi onaylayamaz; başka bir yetkili gerekir.", ErrorKind.Forbidden);

    public static readonly Error AlreadyDecided = new("Common.AlreadyDecided", "Bu istek zaten sonuçlandı.", ErrorKind.Conflict);

    private FourEyesDecision(UserId requestedBy, DecisionStatus status, UserId? decidedBy, DateTimeOffset? decidedAt)
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

    public bool IsPending => Status == DecisionStatus.Pending;

    public static FourEyesDecision Request(UserId requestedBy) => new(requestedBy, DecisionStatus.Pending, null, null);

    public static FourEyesDecision Restore(UserId requestedBy, DecisionStatus status, UserId? decidedBy, DateTimeOffset? decidedAt) =>
        new(requestedBy, status, decidedBy, decidedAt);

    /// <summary>Kararın verilebilir olup olmadığını kontrol eder; durumu değiştirmez.</summary>
    public Result CanDecide(UserId by)
    {
        if (!IsPending)
        {
            return AlreadyDecided;
        }

        return by == RequestedBy ? SameUser : Result.Success();
    }

    public Result Decide(UserId by, bool approve, DateTimeOffset at)
    {
        var allowed = CanDecide(by);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        Status = approve ? DecisionStatus.Approved : DecisionStatus.Rejected;
        DecidedBy = by;
        DecidedAt = at;
        return Result.Success();
    }
}
