using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Auth;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Features.Shell;

public sealed class ShellViewModelTests
{
    private readonly IIdentityApi _identity = Substitute.For<IIdentityApi>();
    private readonly SessionAccess _access = new();
    private readonly IScreenOpener _opener = Substitute.For<IScreenOpener>();
    private readonly IWindowService _windows = Substitute.For<IWindowService>();
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly IAppNavigator _navigator = Substitute.For<IAppNavigator>();
    private readonly CashierShiftContext _shift = new();

    private static ScreenDescriptor Screen(string code, bool showInMenu = true) =>
        new(code, () => code, (_, _) => throw new InvalidOperationException(), showInMenu);

    private ShellViewModel CreateSut()
    {
        var catalog = new ScreenCatalog(
        [
            Screen(ScreenCodes.CageShift),
            Screen(ScreenCodes.CageChips),
            Screen(ScreenCodes.PlayerSearch),
            Screen(ScreenCodes.PlayerProfile, showInMenu: false),
            Screen(ScreenCodes.LiveTables),
        ]);
        var terminal = Substitute.For<ITerminalInfo>();
        terminal.Name.Returns("KASA-1");
        return new ShellViewModel(_identity, _access, catalog, _opener, _windows, _auth, _navigator, _shift, terminal, [_initializer]);
    }

    private readonly ISessionInitializer _initializer = Substitute.For<ISessionInitializer>();

    [Fact]
    public async Task Menu_yuklenince_oturum_hazirligi_yetkilerle_calisir_ve_hatasi_gosterilir()
    {
        MenuReturns(new MenuItemDto(ScreenCodes.CageShift, ["view", "open"]));
        _initializer.InitializeAsync(Arg.Any<UserAccess>(), Arg.Any<CancellationToken>())
            .Returns(TestSupport.Problem(503, "Common.Unavailable", "Vardiya bilgisi alınamadı."));
        var sut = CreateSut();

        await sut.LoadMenuCommand.ExecuteAsync(null);

        await _initializer.Received(1).InitializeAsync(
            Arg.Is<UserAccess>(a => a.CanDo(ScreenCodes.CageShift, "open")), Arg.Any<CancellationToken>());
        Assert.Single(sut.MenuEntries);
        Assert.Equal("Vardiya bilgisi alınamadı.", sut.ErrorMessage);
    }

    private void MenuReturns(params MenuItemDto[] items) =>
        _identity.GetMyMenuAsync(Arg.Any<CancellationToken>()).Returns(ApiResult.Success<IReadOnlyList<MenuItemDto>>(items));

    [Fact]
    public async Task Menu_yalnizca_yetkili_ve_bu_istemcide_olan_ekranlari_gosterir()
    {
        MenuReturns(
            new MenuItemDto(ScreenCodes.CageChips, ["sell"]),
            new MenuItemDto(ScreenCodes.PlayerSearch, []),
            new MenuItemDto(ScreenCodes.PlayerProfile, []),
            new MenuItemDto("admin.roles", ["operate"]));
        var sut = CreateSut();

        await sut.LoadMenuCommand.ExecuteAsync(null);

        Assert.Equal([ScreenCodes.CageChips, ScreenCodes.PlayerSearch], sut.MenuEntries.Select(e => e.ScreenCode));
        Assert.DoesNotContain(sut.MenuEntries, e => e.ScreenCode == ScreenCodes.CageShift);
        Assert.True(_access.Current.CanDo(ScreenCodes.CageChips, "sell"));
        Assert.False(_access.Current.CanDo(ScreenCodes.CageChips, "redeem"));
        Assert.True(_access.Current.CanView(ScreenCodes.PlayerProfile));
    }

    [Fact]
    public async Task Menu_alinamazsa_hicbir_ekran_gosterilmez_ve_hata_gosterilir()
    {
        _identity.GetMyMenuAsync(Arg.Any<CancellationToken>())
            .Returns(ApiResult.Failure<IReadOnlyList<MenuItemDto>>(TestSupport.Problem(401, "Auth", "Oturum sona erdi.")));
        var sut = CreateSut();

        await sut.LoadMenuCommand.ExecuteAsync(null);

        Assert.Empty(sut.MenuEntries);
        Assert.Equal("Oturum sona erdi.", sut.ErrorMessage);
        Assert.False(_access.Current.CanView(ScreenCodes.CageChips));
    }

    [Fact]
    public async Task Filtre_menuyu_daraltir()
    {
        MenuReturns(new MenuItemDto(ScreenCodes.CageChips, []), new MenuItemDto(ScreenCodes.LiveTables, []));
        var sut = CreateSut();
        await sut.LoadMenuCommand.ExecuteAsync(null);

        sut.Filter = "live";

        Assert.Equal([ScreenCodes.LiveTables], sut.MenuEntries.Select(e => e.ScreenCode));
        Assert.Equal(ScreenCodes.LiveTables, sut.SelectedEntry?.ScreenCode);
    }

    [Fact]
    public async Task Cikis_pencereleri_kapatir_yetkiyi_ve_vardiyayi_temizler()
    {
        MenuReturns(new MenuItemDto(ScreenCodes.CageChips, []));
        var sut = CreateSut();
        await sut.LoadMenuCommand.ExecuteAsync(null);
        _shift.Attach(Guid.NewGuid());

        await sut.LogoutCommand.ExecuteAsync(null);

        _windows.Received(1).CloseAll();
        await _auth.Received(1).LogoutAsync(Arg.Any<CancellationToken>());
        _navigator.Received(1).ShowLogin();
        Assert.False(_access.Current.CanView(ScreenCodes.CageChips));
        Assert.Null(_shift.ShiftId);
    }
}
