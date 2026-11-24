using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sgt.Api;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Locking;
using Sgt.DemoData;

namespace Sgt.Api.Tests.DemoData;

/// <summary>Demo saatiyle çalışan host. Outbox arka plan servisleri kapalıdır; üretici outbox'ı kendisi boşaltır.</summary>
public sealed class DemoApiFactory : ApiFactory
{
    public DemoTimeline Timeline { get; } = DemoTimeline.Ending(new SystemClock().UtcNow, DemoScale.Small);

    public DemoClock Clock => _clock ??= new DemoClock(Timeline.SetupStart);

    private DemoClock? _clock;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<IHostedService>();
        services.AddDemoData(Clock);
        services.AddPostgresAdvisoryLock(ConnectionString);
        services.AddScoped<IDemoTenantBootstrap, IdentityDemoTenantBootstrap>();
    }
}

/// <summary>Small ölçekli demo verisi bir kez üretilir; demo testleri bu veriyi paylaşır.</summary>
public sealed class DemoSeedFixture : IAsyncLifetime
{
    public DemoApiFactory Api { get; } = new();

    public DemoSeedResult FirstRun { get; private set; } = null!;

    public DemoSeedOptions Options => new(ApiFactory.Tenant, DemoScale.Small, Api.Timeline);

    public async Task InitializeAsync()
    {
        await Api.InitializeAsync();
        FirstRun = await Generate();
    }

    public async Task<DemoSeedResult> Generate(DemoSeedOptions? options = null)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DemoDataGenerator>().Run(options ?? Options, default);
    }

    public async Task<TResult> Query<TQuery, TResult>(TQuery query)
    {
        await using var scope = Api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>().Handle(query, default);
    }

    public Task DisposeAsync() => Api.DisposeAsync();
}

[CollectionDefinition(Name)]
public sealed class DemoSeedGroup : ICollectionFixture<DemoSeedFixture>
{
    public const string Name = "Demo verisi";
}
