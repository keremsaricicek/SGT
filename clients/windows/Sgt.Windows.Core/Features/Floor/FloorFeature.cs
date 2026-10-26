using Microsoft.Extensions.DependencyInjection;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Features.Floor;

/// <summary>Canlı oyun özelliğinin servis ve ekran kayıtları.</summary>
public static class FloorFeature
{
    public static IServiceCollection AddFloorFeature(this IServiceCollection services)
    {
        services.AddTransient<FloorApiClient>();
        services.AddTransient<ITablesApi>(sp => sp.GetRequiredService<FloorApiClient>());
        services.AddTransient<IRatingsApi>(sp => sp.GetRequiredService<FloorApiClient>());
        services.AddScreen<TablesViewModel>(ScreenCodes.LiveTables, () => Strings.Screen_Tables);
        services.AddScreen<RatingViewModel>(ScreenCodes.LiveRating, () => Strings.Screen_Rating);
        return services;
    }
}
