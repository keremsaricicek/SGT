using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Login;
using Sgt.Windows.Core.Features.Shell;

namespace Sgt.Windows.Core.Tests;

/// <summary>DI kurulumu: tüm ekranlar ve servisler çözülebilmeli (uygulama açılışında hata çıkmasın).</summary>
public sealed class CompositionTests
{
    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Api:BaseAddress"] = "https://api.test",
                ["Authentication:Authority"] = "https://login.test/realms/sgt",
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSgtWindowsCore(configuration);
        services.AddSingleton(Substitute.For<IWindowService>());
        services.AddSingleton(Substitute.For<IAppNavigator>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    [Fact]
    public void Tum_kayitli_ekranlar_olusturulabilir()
    {
        using var provider = BuildProvider();
        var catalog = provider.GetRequiredService<IScreenCatalog>();

        Assert.Equal(
            [
                ScreenCodes.PlayerSearch, ScreenCodes.PlayerProfile,
                ScreenCodes.CageShift, ScreenCodes.CageSummary, ScreenCodes.CageAdjustment, ScreenCodes.CageChips, ScreenCodes.CageExchange,
                ScreenCodes.LiveTables, ScreenCodes.LiveRating,
            ],
            catalog.All.Select(s => s.ScreenCode));
        foreach (var screen in catalog.All)
        {
            var viewModel = screen.Create(provider, new ScreenRequest(screen.ScreenCode, Guid.NewGuid()));
            Assert.False(string.IsNullOrWhiteSpace(viewModel.Title));
            Assert.False(string.IsNullOrWhiteSpace(screen.Title()));
        }
    }

    [Fact]
    public void Giris_kabuk_ve_api_tasiyicisi_cozulur()
    {
        using var provider = BuildProvider();

        Assert.NotNull(provider.GetRequiredService<LoginViewModel>());
        Assert.NotNull(provider.GetRequiredService<ShellViewModel>());
        Assert.NotNull(provider.GetRequiredService<IApiTransport>());
    }
}
