using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>
/// Demo verisi üretilirken SystemClock yerine kullanılan, yalnızca ileri sarılabilen saat (Spec 012 KK4).
/// Handler'lar zamanı IClock'tan aldığı için geçmiş günlerin hareketi bu saatle yazılır. İş parçacığı güvenlidir.
/// </summary>
public sealed class DemoClock(DateTimeOffset start) : IClock
{
    private long _utcTicks = start.UtcTicks;

    public DateTimeOffset UtcNow => new(Interlocked.Read(ref _utcTicks), TimeSpan.Zero);

    /// <summary>Saati verilen ana getirir; geriye gidilemez.</summary>
    public void Set(DateTimeOffset at)
    {
        var target = at.UtcTicks;
        while (true)
        {
            var current = Interlocked.Read(ref _utcTicks);
            if (target < current)
            {
                throw new ArgumentOutOfRangeException(nameof(at), at, "Demo saati geriye alınamaz.");
            }

            if (Interlocked.CompareExchange(ref _utcTicks, target, current) == current)
            {
                return;
            }
        }
    }

    public void Advance(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Demo saati geriye alınamaz.");
        }

        Interlocked.Add(ref _utcTicks, duration.Ticks);
    }
}
