using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.DemoData.Steps;

namespace Sgt.DemoData;

public static class DemoDataServiceCollectionExtensions
{
    /// <summary>
    /// Demo verisi üreticisini kaydeder ve saati <paramref name="clock"/> ile değiştirir (Spec 012 R2).
    /// Yalnızca <c>--seed-demo</c> modunda ve testlerde çağrılır. Çağıran ayrıca <see cref="IAdvisoryLock"/> ve
    /// <see cref="IDemoTenantBootstrap"/> kaydeder (ikisi de altyapı/modül bilgisi ister).
    /// </summary>
    public static IServiceCollection AddDemoData(this IServiceCollection services, DemoClock clock)
    {
        services.AddOutboxDrainers();
        services.AddSingleton(clock);
        services.AddSingleton<IClock>(clock);
        services.AddSingleton<DemoCommands>();
        services.AddSingleton<DemoOutbox>();
        services.AddSingleton<RateBoard>();
        services.AddSingleton<CageDesk>();
        services.AddSingleton<PitDesk>();
        services.AddSingleton<SlotMeterRound>();
        services.AddSingleton<CompDesk>();
        services.AddSingleton<DayClosing>();
        services.AddTransient<IDemoStep, IdentitySeeder>();
        services.AddTransient<IDemoStep, ConfigurationSeeder>();
        services.AddTransient<IDemoStep, LoyaltySeeder>();
        services.AddTransient<IDemoStep, PlayerSeeder>();
        services.AddTransient<IDemoStep, FloorSeeder>();
        services.AddTransient<IDemoStep, DaySimulator>();
        services.AddTransient<IDemoStep, LiveFloor>();
        services.AddTransient<DemoDataGenerator>();
        return services;
    }
}
