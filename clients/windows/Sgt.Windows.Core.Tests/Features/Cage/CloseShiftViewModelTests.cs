using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Cage;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Features.Cage;

public sealed class CloseShiftViewModelTests
{
    private static readonly Guid ShiftId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly ICageShiftApi _api = Substitute.For<ICageShiftApi>();
    private readonly ICashierShiftContext _shift = TestSupport.ShiftAttached(ShiftId);

    private async Task<CloseShiftViewModel> LoadedSut()
    {
        _api.GetShiftAsync(ShiftId, Arg.Any<CancellationToken>()).Returns(ApiResult.Success(new ShiftSummaryDto(
            ShiftId, Guid.NewGuid(), "KASA-1", DateTimeOffset.UnixEpoch, null, null,
            [new BalanceDto("Cash", "TRY", 1000m), new BalanceDto("Chips", "TRY", 500m)], [], [])));
        _api.CloseShiftAsync(Arg.Any<Guid>(), Arg.Any<CloseShiftRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success());

        var sut = new CloseShiftViewModel(_api, _shift, TestSupport.Access((ScreenCodes.CageShift, ["close"])));
        await sut.LoadCommand.ExecuteAsync(null);
        return sut;
    }

    [Fact]
    public async Task Beklenen_bakiyelerden_sayim_satirlari_olusur()
    {
        var sut = await LoadedSut();

        Assert.Equal(2, sut.Lines.Count);
        Assert.Equal(Instrument.Cash, sut.Lines[0].Instrument);
        Assert.Equal(1000m, sut.Lines[0].Expected);
        Assert.False(sut.CloseCommand.CanExecute(null));
    }

    [Fact]
    public async Task Fark_yoksa_aciklama_gerekmez()
    {
        var sut = await LoadedSut();
        sut.Lines[0].CountedText = "1000";
        sut.Lines[1].CountedText = "500";

        Assert.False(sut.NoteRequired);
        Assert.True(sut.CloseCommand.CanExecute(null));
    }

    [Fact]
    public async Task Fark_varsa_aciklama_olmadan_kapatilamaz()
    {
        var sut = await LoadedSut();
        sut.Lines[0].CountedText = "990";
        sut.Lines[1].CountedText = "500";

        Assert.True(sut.NoteRequired);
        Assert.True(sut.NoteMissing);
        Assert.Equal(-10m, sut.Lines[0].Variance);
        Assert.False(sut.CloseCommand.CanExecute(null));

        sut.Note = "10 TL eksik, kontrol edildi";

        Assert.False(sut.NoteMissing);
        Assert.True(sut.CloseCommand.CanExecute(null));
    }

    [Fact]
    public async Task Kapanis_sayimi_ve_aciklamayi_gonderir_ve_vardiyadan_cikar()
    {
        var sut = await LoadedSut();
        sut.Lines[0].CountedText = "990";
        sut.Lines[1].CountedText = "500";
        sut.Note = "  eksik  ";

        await sut.CloseCommand.ExecuteAsync(null);

        await _api.Received(1).CloseShiftAsync(ShiftId, Arg.Is<CloseShiftRequest>(r =>
            r.Note == "eksik"
            && r.ReasonCode == "SHIFT_CLOSE"
            && r.Counted.Count == 2
            && r.Counted[0] == new DrawerAmount(Instrument.Cash, "TRY", 990m)), Arg.Any<CancellationToken>());
        Assert.Null(_shift.ShiftId);
    }

    [Fact]
    public async Task Sunucu_reddederse_problem_gosterilir_ve_vardiya_korunur()
    {
        var sut = await LoadedSut();
        sut.Lines[0].CountedText = "1000";
        sut.Lines[1].CountedText = "500";
        _api.CloseShiftAsync(Arg.Any<Guid>(), Arg.Any<CloseShiftRequest>(), Arg.Any<CancellationToken>())
            .Returns(ApiResult.Failure<Done>(TestSupport.Problem(409, "Finance.ShiftAlreadyClosed", "Vardiya zaten kapalı.")));

        await sut.CloseCommand.ExecuteAsync(null);

        Assert.Equal("Vardiya zaten kapalı.", sut.ErrorMessage);
        Assert.Equal(ShiftId, _shift.ShiftId);
    }

    [Fact]
    public async Task Kapatma_yetkisi_yoksa_kapatilamaz()
    {
        await LoadedSut();
        var sut = new CloseShiftViewModel(_api, _shift, TestSupport.Access((ScreenCodes.CageShift, ["open"])));
        await sut.LoadCommand.ExecuteAsync(null);
        sut.Lines[0].CountedText = "1000";
        sut.Lines[1].CountedText = "500";

        Assert.False(sut.CloseCommand.CanExecute(null));
    }
}
