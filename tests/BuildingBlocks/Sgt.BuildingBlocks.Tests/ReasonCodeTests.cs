using Sgt.BuildingBlocks;

namespace Sgt.BuildingBlocks.Tests;

public sealed class ReasonCodeTests
{
    [Fact]
    public void Create_normalizes_to_upper_case()
    {
        Assert.Equal("ROLE_SETUP", ReasonCode.Create(" role_setup ").Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_missing_reason(string? value)
    {
        Assert.Equal(ReasonCode.Required, ReasonCode.Create(value).Error);
    }

    [Fact]
    public void Create_rejects_overlong_reason()
    {
        Assert.True(ReasonCode.Create(new string('A', 51)).IsFailure);
    }
}

public sealed class BusinessCalendarTests
{
    [Fact]
    public void Local_today_uses_kktc_time()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 10, 26, 22, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 10, 27), clock.LocalToday());
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
