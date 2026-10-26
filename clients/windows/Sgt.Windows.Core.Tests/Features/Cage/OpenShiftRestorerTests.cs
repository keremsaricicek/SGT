using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Cage;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Features.Cage;

public sealed class OpenShiftRestorerTests
{
    private readonly ICageShiftApi _api = Substitute.For<ICageShiftApi>();
    private readonly CashierShiftContext _shift = new();

    private static UserAccess Access(params (string Screen, string[] Actions)[] screens) => TestSupport.Access(screens).Current;

    [Fact]
    public async Task Acik_vardiya_varsa_otomatik_baglanir()
    {
        var shiftId = Guid.NewGuid();
        _api.GetMyOpenShiftAsync(Arg.Any<CancellationToken>()).Returns(ApiResult.Success<CreatedDto?>(new CreatedDto(shiftId)));

        var problem = await new OpenShiftRestorer(_api, _shift).InitializeAsync(Access((ScreenCodes.CageShift, ["view"])), CancellationToken.None);

        Assert.Null(problem);
        Assert.Equal(shiftId, _shift.ShiftId);
    }

    [Fact]
    public async Task Acik_vardiya_yoksa_bagli_kalmaz()
    {
        _api.GetMyOpenShiftAsync(Arg.Any<CancellationToken>()).Returns(ApiResult.Success<CreatedDto?>(null));

        await new OpenShiftRestorer(_api, _shift).InitializeAsync(Access((ScreenCodes.CageShift, ["view"])), CancellationToken.None);

        Assert.Null(_shift.ShiftId);
    }

    [Fact]
    public async Task Vardiya_ekrani_yetkisi_yoksa_sunucuya_sorulmaz()
    {
        await new OpenShiftRestorer(_api, _shift).InitializeAsync(Access((ScreenCodes.PlayerSearch, [])), CancellationToken.None);

        await _api.DidNotReceive().GetMyOpenShiftAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sunucu_hatasi_problem_olarak_doner()
    {
        _api.GetMyOpenShiftAsync(Arg.Any<CancellationToken>())
            .Returns(ApiResult.Failure<CreatedDto?>(TestSupport.Problem(403, "Identity.Forbidden", "Yetki yok.")));

        var problem = await new OpenShiftRestorer(_api, _shift).InitializeAsync(Access((ScreenCodes.CageShift, ["view"])), CancellationToken.None);

        Assert.Equal("Identity.Forbidden", problem?.Code);
        Assert.Null(_shift.ShiftId);
    }
}
