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

    private static ApiResult<IReadOnlyList<RunningRatingDto>> Running(params RunningRatingDto[] ratings) =>
        ApiResult.Success<IReadOnlyList<RunningRatingDto>>(ratings);

    private RatingViewModel RatingSut(PlayerSummaryDto? selected = null) =>
        new(
            _tables, _ratings, TestSupport.Access((ScreenCodes.LiveRating, ["view", "start", "finish"])),
            new PlayerPickerViewModel(Substitute.For<IPlayersApi>()) { Selected = selected });

    [Fact]
    public async Task Acilista_suren_ratingler_sunucudan_gelir()
    {
        var other = new RunningRatingDto(Guid.NewGuid(), Guid.Parse("abcdef01-0000-0000-0000-000000000000"), OpenTable.TableId, "RL-02", DateTimeOffset.UnixEpoch);
        _ratings.ListRunningAsync(null, Arg.Any<CancellationToken>()).Returns(Running(other));
        var sut = RatingSut();

        await sut.InitializeAsync(CancellationToken.None);

        var row = Assert.Single(sut.ActiveRatings);
        Assert.Equal(other.RatingId, row.RatingId);
        Assert.Equal("RL-02", row.TableCode);
        Assert.Equal("#abcdef01", row.PlayerName);
        Assert.Equal(row, sut.SelectedRating);
    }

    [Fact]
    public async Task Rating_baslatilir_ve_ortalama_bahisle_bitirilir()
    {
        var ratingId = Guid.NewGuid();
        var player = new PlayerSummaryDto(Guid.NewGuid(), "P-1", "Ada", "Test", null, false);
        var running = new RunningRatingDto(ratingId, player.PlayerId, OpenTable.TableId, OpenTable.Code, DateTimeOffset.UnixEpoch);
        _ratings.ListRunningAsync(null, Arg.Any<CancellationToken>()).Returns(Running(), Running(running), Running());
        _ratings.StartRatingAsync(Arg.Any<StartRatingRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success(new CreatedDto(ratingId)));
        _ratings.FinishRatingAsync(ratingId, Arg.Any<RatingValuesRequest>(), Arg.Any<CancellationToken>()).Returns(ApiResult.Success());
        var sut = RatingSut(player);
        await sut.InitializeAsync(CancellationToken.None);

        Assert.Equal(OpenTable, Assert.Single(sut.OpenTables));
        Assert.Empty(sut.ActiveRatings);
        await sut.StartCommand.ExecuteAsync(null);
        var active = Assert.Single(sut.ActiveRatings);
        Assert.Equal(ratingId, active.RatingId);
        Assert.Equal("Ada Test", active.PlayerName);

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
