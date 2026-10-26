using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Domain;

/// <summary>Oyuncu kısıtlaması. Kaldırma kaydı silmez, kaldıranı ve zamanı işaretler.</summary>
public sealed class PlayerRestriction
{
    internal PlayerRestriction(
        Guid id, RestrictionType type, string? note, DateTimeOffset? until, UserId addedBy, DateTimeOffset addedAt,
        UserId? removedBy = null, DateTimeOffset? removedAt = null)
    {
        Id = id;
        Type = type;
        Note = note;
        Until = until;
        AddedBy = addedBy;
        AddedAt = addedAt;
        RemovedBy = removedBy;
        RemovedAt = removedAt;
    }

    public Guid Id { get; }

    public RestrictionType Type { get; }

    public string? Note { get; }

    /// <summary>Bitiş; null ise süresiz.</summary>
    public DateTimeOffset? Until { get; }

    public UserId AddedBy { get; }

    public DateTimeOffset AddedAt { get; }

    public UserId? RemovedBy { get; private set; }

    public DateTimeOffset? RemovedAt { get; private set; }

    public bool IsActiveAt(DateTimeOffset at) => RemovedAt is null && (Until is null || Until > at);

    public static PlayerRestriction Restore(
        Guid id, RestrictionType type, string? note, DateTimeOffset? until, UserId addedBy, DateTimeOffset addedAt,
        UserId? removedBy, DateTimeOffset? removedAt) =>
        new(id, type, note, until, addedBy, addedAt, removedBy, removedAt);

    internal void Remove(UserId by, DateTimeOffset at)
    {
        RemovedBy = by;
        RemovedAt = at;
    }
}
