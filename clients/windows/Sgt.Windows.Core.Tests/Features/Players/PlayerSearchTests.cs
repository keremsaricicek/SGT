using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;

namespace Sgt.Windows.Core.Tests.Features.Players;

public sealed class PlayerSearchTests
{
    private readonly IPlayersApi _api = Substitute.For<IPlayersApi>();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData(" a ")]
    public void Iki_karakterden_kisa_terimle_arama_yapilamaz(string? term)
    {
        var sut = new PlayerPickerViewModel(_api) { Term = term };

        Assert.True(sut.TermTooShort);
        Assert.False(sut.SearchCommand.CanExecute(null));
    }

    [Fact]
    public async Task Iki_karakterle_arama_yapilir_ve_terim_kirpilir()
    {
        var player = new PlayerSummaryDto(Guid.NewGuid(), "P-7", "Ada", "Test", "C-1", false);
        _api.SearchAsync("ad", Arg.Any<CancellationToken>()).Returns(ApiResult.Success<IReadOnlyList<PlayerSummaryDto>>([player]));
        var sut = new PlayerPickerViewModel(_api) { Term = " ad " };

        Assert.True(sut.SearchCommand.CanExecute(null));
        await sut.SearchCommand.ExecuteAsync(null);

        Assert.Equal(player, Assert.Single(sut.Results));
        Assert.Equal(player, sut.Selected);
    }

    [Fact]
    public async Task Arama_hatasi_problem_olarak_gosterilir()
    {
        _api.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ApiResult.Failure<IReadOnlyList<PlayerSummaryDto>>(TestSupport.Problem(403, "Identity.Forbidden", "Yetkiniz yok.")));
        var sut = new PlayerPickerViewModel(_api) { Term = "ada" };

        await sut.SearchCommand.ExecuteAsync(null);

        Assert.Empty(sut.Results);
        Assert.Equal("Yetkiniz yok.", sut.ErrorMessage);
    }

    [Fact]
    public async Task Oyuncu_karti_yalnizca_yetki_varsa_acilir()
    {
        var player = new PlayerSummaryDto(Guid.NewGuid(), "P-7", "Ada", "Test", null, false);
        var opener = Substitute.For<IScreenOpener>();
        var withoutProfile = new PlayerSearchViewModel(
            new PlayerPickerViewModel(_api) { Selected = player }, opener, TestSupport.Access((ScreenCodes.PlayerSearch, [])));
        var withProfile = new PlayerSearchViewModel(
            new PlayerPickerViewModel(_api) { Selected = player },
            opener,
            TestSupport.Access((ScreenCodes.PlayerSearch, []), (ScreenCodes.PlayerProfile, [])));

        Assert.False(withoutProfile.OpenDetailCommand.CanExecute(null));
        Assert.True(withProfile.OpenDetailCommand.CanExecute(null));

        await withProfile.OpenDetailCommand.ExecuteAsync(null);
        await opener.Received(1).OpenAsync(new ScreenRequest(ScreenCodes.PlayerProfile, player.PlayerId), Arg.Any<CancellationToken>());
    }
}
