using NSubstitute;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Floor;
using Sgt.Windows.Core.Features.Players;
using Sgt.Windows.Core.Features.Shell;

namespace Sgt.Windows.Core.Tests.Features.Floor;

public sealed class FloorViewModelTests
{
    private static readonly TableDto ClosedTable = new(Guid.NewGuid(), "BJ-01", "Blackjack", "TRY", 100, 5000, 2, 60, false);
    private static readonly TableDto OpenTable = new(Guid.NewGuid(), "RL-02", "Roulette", "TRY", 50, 2000, 2.7m, 40, true);

    private readonly ITablesApi _tables = Substitute.For<ITablesApi>();
    private readonly IRatingsApi _ratings = Substitute.For<IRatingsApi>();

    public FloorViewModelTests() =>
        _tables.ListTablesAsync(Arg.Any<CancellationToken>()).Returns(ApiResult.Success<IReadOnlyList<TableDto>>([ClosedTable, OpenTable]));

    [Fact]
    public async Task Masa_acma_yalnizca_yetki_varsa_ve_masa_kapaliysa_mumkun()
    {
        var sut = new TablesViewModel(_tables, TestSupport.Access((ScreenCodes.LiveTables, []), (ScreenCodes.LiveTableSession, ["open"])));
        await sut.InitializeAsync(CancellationToken.None);

        sut.SelectedTable = ClosedTable;
        Assert.True(sut.OpenTableCommand.CanExecute(null));
        Assert.False(sut.CloseTableCommand.CanExecute(null));

        sut.SelectedTable = OpenTable;
        Assert.False(sut.OpenTableCommand.CanExecute(null));
        Assert.False(sut.CloseTableCommand.CanExecute(null));
    }

    [Fact]
    public async Task Masa_acilinca_liste_yenilenir()
    {
        _tables.OpenTableAsync(ClosedTable.TableId, Arg.Any<FloorReasonRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success());
        var sut = new TablesViewModel(_tables, TestSupport.Access((ScreenCodes.LiveTableSession, ["open"])));
        await sut.InitializeAsync(CancellationToken.None);
        sut.SelectedTable = ClosedTable;

        await sut.OpenTableCommand.ExecuteAsync(null);

        await _tables.Received(1).OpenTableAsync(ClosedTable.TableId, new FloorReasonRequest("TABLE_SESSION"), Arg.Any<CancellationToken>());
        await _tables.Received(2).ListTablesAsync(Arg.Any<CancellationToken>());
        Assert.NotNull(sut.StatusMessage);
    }

    [Fact]
    public async Task Rating_baslatilir_ve_ortalama_bahisle_bitirilir()
    {
        var ratingId = Guid.NewGuid();
        var player = new PlayerSummaryDto(Guid.NewGuid(), "P-1", "Ada", "Test", null, false);
        _ratings.StartRatingAsync(Arg.Any<StartRatingRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success(new CreatedDto(ratingId)));
        _ratings.FinishRatingAsync(ratingId, Arg.Any<RatingValuesRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success());
        var sut = new RatingViewModel(
            _tables, _ratings, TestSupport.Access((ScreenCodes.LiveRating, ["start", "finish"])), TimeProvider.System,
            new PlayerPickerViewModel(Substitute.For<IPlayersApi>()) { Selected = player });
        await sut.InitializeAsync(CancellationToken.None);

        Assert.Equal(OpenTable, Assert.Single(sut.OpenTables));
        await sut.StartCommand.ExecuteAsync(null);
        Assert.Equal(ratingId, Assert.Single(sut.ActiveRatings).RatingId);

        Assert.False(sut.FinishCommand.CanExecute(null));
        sut.AverageBetText = "250";
        sut.PlayerResultText = "-300";
        Assert.True(sut.FinishCommand.CanExecute(null));
        await sut.FinishCommand.ExecuteAsync(null);

        await _ratings.Received(1).StartRatingAsync(new StartRatingRequest(OpenTable.TableId, player.PlayerId, "RATING"), Arg.Any<CancellationToken>());
        await _ratings.Received(1).FinishRatingAsync(ratingId, new RatingValuesRequest(250m, -300m, "RATING"), Arg.Any<CancellationToken>());
        Assert.Empty(sut.ActiveRatings);
    }
}
