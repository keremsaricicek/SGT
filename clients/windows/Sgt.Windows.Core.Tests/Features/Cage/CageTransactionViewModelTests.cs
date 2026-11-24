using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Cage;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Features.Cage;

public sealed class CageTransactionViewModelTests
{
    private static readonly Guid ShiftId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly PlayerSummaryDto Player = new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "P-1", "Test", "Oyuncu", null, false);

    private readonly ICageTransactionApi _api = Substitute.For<ICageTransactionApi>();
    private readonly List<CageRequest> _requests = [];
    private readonly List<Guid?> _correlations = [];
    private readonly CorrelationContext _correlation = new();

    private CageTransactionViewModel CreateSut(ISessionAccess? access = null, ICashierShiftContext? shift = null)
    {
        _api.SellChipsAsync(Arg.Any<Guid>(), Arg.Do<CageRequest>(r =>
        {
            _requests.Add(r);
            _correlations.Add(_correlation.Current);
        }), Arg.Any<CancellationToken>()).Returns(ApiResult.Failure<CageTransactionDto>(TestSupport.Problem(409, "X", "x")));

        var picker = new PlayerPickerViewModel(Substitute.For<IPlayersApi>()) { Selected = Player };
        return new CageTransactionViewModel(
            "Çip",
            [new SellChips(), new RedeemChips()],
            _api,
            shift ?? TestSupport.ShiftAttached(ShiftId),
            access ?? TestSupport.Access((ScreenCodes.CageChips, ["sell", "redeem"])),
            _correlation,
            new SequentialIds(),
            picker)
        {
            AmountText = "100",
            SelectedCurrency = Currency.Eur,
        };
    }

    private void ApiReturns(params ApiResult<CageTransactionDto>[] results) =>
        _api.SellChipsAsync(Arg.Any<Guid>(), Arg.Any<CageRequest>(), Arg.Any<CancellationToken>())
            .Returns(results[0], results[1..]);

    private static CageTransactionDto Done(Guid transactionId) => new(transactionId, "ChipSale", 100, "EUR", null, []);

    [Fact]
    public async Task Tekrar_denemede_ayni_TransactionId_gonderilir_ve_problem_details_gosterilir()
    {
        var sut = CreateSut();
        var problem = new ApiProblem(503, "Common.Unavailable", "Hizmet geçici olarak kullanılamıyor", "Sunucu meşgul, tekrar deneyin.");
        ApiReturns(ApiResult.Failure<CageTransactionDto>(problem), ApiResult.Success(Done(Guid.Empty)));

        await sut.SubmitCommand.ExecuteAsync(null);

        Assert.Equal("Sunucu meşgul, tekrar deneyin.", sut.ErrorMessage);
        Assert.Equal("Common.Unavailable", sut.ErrorCode);
        Assert.NotNull(sut.PendingTransactionId);

        await sut.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(2, _requests.Count);
        Assert.Equal(_requests[0].TransactionId, _requests[1].TransactionId);
        Assert.Equal(_correlations[0], _correlations[1]);
        Assert.NotNull(_correlations[0]);
        Assert.Null(sut.ErrorMessage);
        Assert.Null(sut.PendingTransactionId);
        Assert.NotNull(sut.StatusMessage);
    }

    [Fact]
    public async Task Basarili_islemden_sonra_yeni_islem_yeni_TransactionId_alir()
    {
        var sut = CreateSut();
        ApiReturns(ApiResult.Success(Done(Guid.Empty)), ApiResult.Success(Done(Guid.Empty)));

        await sut.SubmitCommand.ExecuteAsync(null);
        sut.AmountText = "100";
        await sut.SubmitCommand.ExecuteAsync(null);

        Assert.NotEqual(_requests[0].TransactionId, _requests[1].TransactionId);
    }

    [Fact]
    public async Task Hatadan_sonra_tutar_degisirse_yeni_islem_sayilir()
    {
        var sut = CreateSut();
        ApiReturns(ApiResult.Failure<CageTransactionDto>(TestSupport.Problem(400, "Finance.Invalid", "Geçersiz")), ApiResult.Success(Done(Guid.Empty)));

        await sut.SubmitCommand.ExecuteAsync(null);
        sut.AmountText = "250";
        await sut.SubmitCommand.ExecuteAsync(null);

        Assert.NotEqual(_requests[0].TransactionId, _requests[1].TransactionId);
        Assert.Equal(250m, _requests[1].Amount);
    }

    [Fact]
    public async Task Istek_sozlesmedeki_alanlari_tasir()
    {
        var sut = CreateSut();
        ApiReturns(ApiResult.Success(Done(Guid.Empty)), ApiResult.Success(Done(Guid.Empty)));

        await sut.SubmitCommand.ExecuteAsync(null);

        var request = Assert.Single(_requests);
        Assert.Equal(Player.PlayerId, request.PlayerId);
        Assert.Equal(100m, request.Amount);
        Assert.Equal("EUR", request.Currency);
        Assert.Equal("CHIP_SALE", request.ReasonCode);
        await _api.Received(1).SellChipsAsync(ShiftId, Arg.Any<CageRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Yetki_yoksa_onay_kapali()
    {
        var sut = CreateSut(access: TestSupport.Access((ScreenCodes.CageChips, ["redeem"])));
        sut.SelectedKind = sut.Kinds.OfType<SellChips>().Single();

        Assert.False(sut.SubmitCommand.CanExecute(null));
    }

    [Fact]
    public void Acik_vardiya_yoksa_onay_kapali()
    {
        var sut = CreateSut(shift: new CashierShiftContext());

        Assert.False(sut.SubmitCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("abc")]
    public void Gecersiz_tutarla_onay_kapali(string amount)
    {
        var sut = CreateSut();
        sut.AmountText = amount;

        Assert.False(sut.SubmitCommand.CanExecute(null));
    }

    [Fact]
    public void Gecerli_girdilerle_onay_acik()
    {
        var sut = CreateSut();

        Assert.True(sut.SubmitCommand.CanExecute(null));
    }
}
