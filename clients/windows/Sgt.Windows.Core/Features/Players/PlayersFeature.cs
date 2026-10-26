using Microsoft.Extensions.DependencyInjection;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Features.Players;

/// <summary>Oyuncu özelliğinin servis ve ekran kayıtları.</summary>
public static class PlayersFeature
{
    public static IServiceCollection AddPlayersFeature(this IServiceCollection services)
    {
        services.AddTransient<IPlayersApi, PlayersApiClient>();
        services.AddTransient<PlayerPickerViewModel>();
        services.AddScreen<PlayerSearchViewModel>(ScreenCodes.PlayerSearch, () => Strings.Screen_PlayerSearch);
        services.AddSingleton(new ScreenDescriptor(
            ScreenCodes.PlayerProfile,
            () => Strings.Screen_PlayerProfile,
            (sp, request) => new PlayerDetailViewModel(
                sp.GetRequiredService<IPlayersApi>(),
                request.EntityId ?? throw new ArgumentException("Oyuncu kimliği gerekli.", nameof(request))),
            ShowInMenu: false));
        return services;
    }
}
