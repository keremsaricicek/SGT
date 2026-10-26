namespace Sgt.BuildingBlocks;

/// <summary>Gerçek sistem saati. Yalnızca host tarafında DI ile kaydedilir.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
