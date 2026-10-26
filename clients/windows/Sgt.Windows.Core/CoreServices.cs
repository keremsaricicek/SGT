using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Auth;
using Sgt.Windows.Core.Configuration;
using Sgt.Windows.Core.Features.Cage;
using Sgt.Windows.Core.Features.Floor;
using Sgt.Windows.Core.Features.Login;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core;

/// <summary>
/// Windows istemcisinin çekirdek servisleri. WPF uygulaması yalnızca <see cref="IWindowService"/> ve
/// <see cref="IAppNavigator"/> gerçeklemelerini ekler.
/// </summary>
public static class CoreServices
{
    public static IServiceCollection AddSgtWindowsCore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.Section));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.Section));
        services.Configure<UiOptions>(configuration.GetSection(UiOptions.Section));

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IIdGenerator, GuidIdGenerator>();
        services.AddSingleton<ITerminalInfo, MachineTerminalInfo>();
        services.AddSingleton<ICorrelationContext, CorrelationContext>();
        services.AddSingleton<ICashierShiftContext, CashierShiftContext>();
        services.AddSingleton<ISessionAccess, SessionAccess>();

        services.AddSingleton<ISystemBrowser, ShellSystemBrowser>();
        services.AddSingleton<OidcAuthService>();
        services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<OidcAuthService>());
        services.AddSingleton<ITokenProvider>(sp => sp.GetRequiredService<OidcAuthService>());

        services.AddTransient<SgtHeadersHandler>();
        services.AddTransient<BearerTokenHandler>();
        services.AddHttpClient<IApiTransport, ApiTransport>((sp, http) =>
            {
                var options = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
                var baseAddress = options.BaseAddress ?? throw new InvalidOperationException("Api:BaseAddress ayarı eksik.");
                http.BaseAddress = baseAddress.AbsoluteUri.EndsWith('/') ? baseAddress : new Uri(baseAddress.AbsoluteUri + "/");
                http.Timeout = options.Timeout;
            })
            .AddHttpMessageHandler<BearerTokenHandler>()
            .AddHttpMessageHandler<SgtHeadersHandler>();

        services.AddTransient<IIdentityApi, IdentityApiClient>();
        services.AddSingleton<IScreenCatalog, ScreenCatalog>();
        services.AddSingleton<IScreenOpener, ScreenOpener>();
        services.AddTransient<LoginViewModel>();
        services.AddSingleton<ShellViewModel>();

        services.AddPlayersFeature();
        services.AddCageFeature();
        services.AddFloorFeature();
        return services;
    }
}
