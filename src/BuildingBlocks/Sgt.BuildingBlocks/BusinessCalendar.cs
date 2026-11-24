namespace Sgt.BuildingBlocks;

/// <summary>İşletmenin yerel saati (KKTC). Takvim günü gerektiren kurallar (yaş, gün sonu) bunu kullanır.</summary>
public static class BusinessCalendar
{
    public static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Famagusta");

    public static DateOnly LocalToday(this IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.UtcNow, TimeZone).DateTime);
    }
}
