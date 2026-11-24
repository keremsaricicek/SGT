namespace Sgt.DemoData.Tests;

public sealed class DemoClockTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 22, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Starts_at_given_instant()
    {
        Assert.Equal(Start, new DemoClock(Start).UtcNow);
    }

    [Fact]
    public void Set_moves_forward_to_given_instant()
    {
        var clock = new DemoClock(Start);

        clock.Set(Start.AddHours(5));

        Assert.Equal(Start.AddHours(5), clock.UtcNow);
    }

    [Fact]
    public void Advance_adds_duration()
    {
        var clock = new DemoClock(Start);

        clock.Advance(TimeSpan.FromMinutes(90));
        clock.Advance(TimeSpan.FromMinutes(30));

        Assert.Equal(Start.AddHours(2), clock.UtcNow);
    }

    [Fact]
    public void Never_goes_backwards()
    {
        var clock = new DemoClock(Start);
        clock.Set(Start.AddDays(1));

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Set(Start));
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(TimeSpan.FromSeconds(-1)));
        Assert.Equal(Start.AddDays(1), clock.UtcNow);
    }

    [Fact]
    public void Setting_same_instant_is_allowed()
    {
        var clock = new DemoClock(Start);

        clock.Set(Start);

        Assert.Equal(Start, clock.UtcNow);
    }

    [Fact]
    public void Returns_utc_even_when_given_local_offset()
    {
        var clock = new DemoClock(new DateTimeOffset(2026, 10, 22, 11, 0, 0, TimeSpan.FromHours(3)));

        Assert.Equal(TimeSpan.Zero, clock.UtcNow.Offset);
        Assert.Equal(Start, clock.UtcNow);
    }
}
