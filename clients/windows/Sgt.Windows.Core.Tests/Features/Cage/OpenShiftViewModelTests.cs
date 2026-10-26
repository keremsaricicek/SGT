using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Cage;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Features.Cage;

public sealed class OpenShiftViewModelTests
{
    private readonly ICageShiftApi _api = Substitute.For<ICageShiftApi>();
    private readonly CashierShiftContext _shift = new();

    private OpenShiftViewModel CreateSut(params string[] actions) =>
        new(_api, _shift, TestSupport.Access((ScreenCodes.CageShift, actions)));

    [Fact]
    public async Task Acilis_cekmecesiyle_vardiya_acar_ve_baglanir()
    {
        var shiftId = Guid.NewGuid();
        _api.OpenShiftAsync(Arg.Any<OpenShiftRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success(new CreatedDto(shiftId)));
        var sut = CreateSut("open");
        sut.Lines[0].AmountText = "5000";
        sut.Lines[1].AmountText = "0";

        Assert.True(sut.OpenCommand.CanExecute(null));
        await sut.OpenCommand.ExecuteAsync(null);

        await _api.Received(1).OpenShiftAsync(
            Arg.Is<OpenShiftRequest>(r => r.Opening.Count == 2 && r.Opening[0].Amount == 5000m && r.Opening[1].Instrument == Instrument.Chips),
            Arg.Any<CancellationToken>());
        Assert.Equal(shiftId, _shift.ShiftId);
    }

    [Fact]
    public void Tutar_girilmeden_veya_yetkisiz_acilamaz()
    {
        var withoutAmounts = CreateSut("open");
        Assert.False(withoutAmounts.OpenCommand.CanExecute(null));

        var withoutPermission = CreateSut("close");
        withoutPermission.Lines[0].AmountText = "1";
        withoutPermission.Lines[1].AmountText = "1";
        Assert.False(withoutPermission.OpenCommand.CanExecute(null));
    }
}
