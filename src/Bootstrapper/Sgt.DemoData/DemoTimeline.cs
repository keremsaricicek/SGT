using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>
/// Demo verisinin zaman çizelgesi (Spec 012 KK4): bugünden önceki <see cref="Days"/> gün simüle edilir,
/// sonra saat gerçek ana (<see cref="Now"/>) getirilir. Saatler KKTC yerel saatidir, UTC'ye çevrilir.
/// </summary>
public sealed record DemoTimeline(DateOnly Today, int Days, DateTimeOffset Now)
{
    private static readonly TimeOnly SetupTime = new(6, 0);

    /// <summary>Gerçek an ve ölçekten çizelge kurar.</summary>
    public static DemoTimeline Ending(DateTimeOffset now, DemoScale scale)
    {
        ArgumentNullException.ThrowIfNull(scale);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, BusinessCalendar.TimeZone).DateTime);
        return new DemoTimeline(today, scale.Days, now.ToUniversalTime());
    }

    public DateOnly FirstDay => Today.AddDays(-Days);

    /// <summary>Tanımların (kullanıcı, kur, oyuncu, masa) yazıldığı an: ilk günün sabahı.</summary>
    public DateTimeOffset SetupStart => At(FirstDay, SetupTime);

    /// <summary>Simüle edilen geçmiş günler (bugün hariç), eskiden yeniye.</summary>
    public IEnumerable<DateOnly> PastDays => Enumerable.Range(0, Days).Select(FirstDay.AddDays);

    /// <summary>Yerel gün + saat → UTC an.</summary>
    public static DateTimeOffset At(DateOnly day, TimeOnly localTime)
    {
        var local = day.ToDateTime(localTime);
        return new DateTimeOffset(local, BusinessCalendar.TimeZone.GetUtcOffset(local)).ToUniversalTime();
    }
}
