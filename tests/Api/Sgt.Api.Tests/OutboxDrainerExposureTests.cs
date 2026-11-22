using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.Api.Tests;

/// <summary>Spec 012: anında outbox boşaltma yalnızca --seed-demo modunda vardır; normal API host'unda çözülemez.</summary>
public sealed class OutboxDrainerExposureTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    [Fact]
    public void Regular_api_host_does_not_register_outbox_drainers()
    {
        Assert.Null(api.Services.GetService<OutboxDrainers>());
        Assert.Null(api.Services.GetService<IOutboxDrainer>());
    }
}
