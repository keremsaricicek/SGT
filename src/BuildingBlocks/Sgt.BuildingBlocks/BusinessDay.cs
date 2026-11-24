namespace Sgt.BuildingBlocks;

/// <summary>Bir iş günü: KKTC yerel saatiyle [Start, End) aralığı, UTC olarak (Spec 011 KK1).</summary>
public sealed record BusinessDay(DateOnly Date, DateTimeOffset Start, DateTimeOffset End)
{
    public static BusinessDay For(DateOnly date)
    {
        var start = Local(date);
        return new BusinessDay(date, start, Local(date.AddDays(1)));
    }

    private static DateTimeOffset Local(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(local, BusinessCalendar.TimeZone.GetUtcOffset(local)).ToUniversalTime();
    }
}
