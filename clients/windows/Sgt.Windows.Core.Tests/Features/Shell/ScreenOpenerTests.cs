using NSubstitute;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Features.Shell;

public sealed class ScreenOpenerTests
{
    private readonly IWindowService _windows = Substitute.For<IWindowService>();
    private readonly IScreenViewModel _viewModel = Substitute.For<IScreenViewModel>();
    private int _created;

    private ScreenOpener CreateSut(ISessionAccess access) =>
        new(
            new ScreenCatalog([new ScreenDescriptor(ScreenCodes.LiveTables, () => "Masalar", (_, _) =>
            {
                _created++;
                return _viewModel;
            })]),
            access,
            _windows,
            Substitute.For<IServiceProvider>());

    [Fact]
    public async Task Gorme_yetkisi_yoksa_pencere_acilmaz()
    {
        var sut = CreateSut(TestSupport.Access((ScreenCodes.CageChips, [])));

        Assert.False(await sut.OpenAsync(new ScreenRequest(ScreenCodes.LiveTables), CancellationToken.None));
        _windows.DidNotReceiveWithAnyArgs().Show(default!, default!);
        Assert.Equal(0, _created);
    }

    [Fact]
    public async Task Yetkiliyse_pencere_acilir_ve_ilk_yukleme_yapilir()
    {
        var sut = CreateSut(TestSupport.Access((ScreenCodes.LiveTables, [])));

        Assert.True(await sut.OpenAsync(new ScreenRequest(ScreenCodes.LiveTables), CancellationToken.None));

        _windows.Received(1).Show(ScreenCodes.LiveTables, _viewModel);
        await _viewModel.Received(1).InitializeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Acik_pencere_tekrar_acilmaz_one_getirilir()
    {
        _windows.TryActivate(ScreenCodes.LiveTables).Returns(true);
        var sut = CreateSut(TestSupport.Access((ScreenCodes.LiveTables, [])));

        Assert.True(await sut.OpenAsync(new ScreenRequest(ScreenCodes.LiveTables), CancellationToken.None));

        Assert.Equal(0, _created);
        _windows.DidNotReceiveWithAnyArgs().Show(default!, default!);
    }

    [Fact]
    public void Kayda_ozel_ekranin_pencere_anahtari_kayit_kimligini_icerir()
    {
        var id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        Assert.Equal("player.profile/44444444-4444-4444-4444-444444444444", new ScreenRequest(ScreenCodes.PlayerProfile, id).WindowKey);
    }
}
