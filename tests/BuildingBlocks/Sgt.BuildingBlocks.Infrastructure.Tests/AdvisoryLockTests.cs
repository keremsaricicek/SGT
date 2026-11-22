using Sgt.BuildingBlocks.Infrastructure.Locking;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed class AdvisoryLockTests(MessagingFixture fx) : IClassFixture<MessagingFixture>
{
    [Fact]
    public async Task Lock_is_exclusive_per_name_until_released()
    {
        var locks = new PostgresAdvisoryLock(fx.ConnectionString);
        var name = $"test:{Guid.NewGuid()}";

        var first = await locks.TryAcquire(name, default);
        Assert.NotNull(first);
        Assert.Null(await locks.TryAcquire(name, default));

        var other = await locks.TryAcquire(name + ":other", default);
        Assert.NotNull(other);
        await other.DisposeAsync();

        await first.DisposeAsync();
        var again = await locks.TryAcquire(name, default);
        Assert.NotNull(again);
        await again.DisposeAsync();
    }
}
