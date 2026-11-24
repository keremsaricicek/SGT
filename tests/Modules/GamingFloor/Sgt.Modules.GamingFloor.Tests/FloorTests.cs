using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Application.Ratings;
using Sgt.Modules.GamingFloor.Application.Tables;
using Sgt.Modules.GamingFloor.Domain;

namespace Sgt.Modules.GamingFloor.Tests;

public sealed class FloorTests
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    private static readonly Guid Player = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
    private static readonly DateTimeOffset T0 = new(2026, 10, 26, 20, 0, 0, TimeSpan.Zero);

    private readonly FakeTables _tables = new();
    private readonly FakeRatings _ratings = new();
    private readonly FakePlayers _players = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly ManualClock _clock = new() { UtcNow = T0 };

    private static AuditContext Audit() => new(UserId.New(), "PIT-1", ReasonCode.Of("PIT"), Guid.NewGuid());

    [Theory]
    [InlineData(0, 100, 2.5, 60)]
    [InlineData(200, 100, 2.5, 60)]
    [InlineData(10, 100, 101, 60)]
    [InlineData(10, 100, 2.5, 0)]
    public void KK1_invalid_table_definitions_are_rejected(decimal min, decimal max, decimal edge, int hands)
    {
        Assert.True(GamingTable.Define(Guid.NewGuid(), Tenant, "BJ-01", GameType.Blackjack, "TRY", min, max, edge, hands, Audit(), T0).IsFailure);
    }

    [Fact]
    public async Task KK1_table_code_is_unique()
    {
        Assert.True((await Define("bj-01")).IsSuccess);
        Assert.Equal(FloorErrors.TableCodeTaken, (await Define("BJ-01")).Error);
    }

    [Fact]
    public async Task KK2_table_opens_once_and_cannot_close_with_running_rating()
    {
        var tableId = (await Define("BJ-02")).Value;
        Assert.True((await Open(tableId)).IsSuccess);
        Assert.Equal(FloorErrors.TableAlreadyOpen, (await Open(tableId)).Error);

        await Start(tableId);

        Assert.Equal(FloorErrors.RatingsStillRunning, (await Close(tableId)).Error);
    }

    [Fact]
    public async Task KK3_rating_needs_open_table_unrestricted_player_and_no_other_running_rating()
    {
        var tableId = (await Define("BJ-03")).Value;
        Assert.Equal(FloorErrors.TableNotOpen, (await Start(tableId)).Error);
        await Open(tableId);

        _players.Restricted = true;
        Assert.Equal(FloorErrors.PlayerRestricted, (await Start(tableId)).Error);
        _players.Restricted = false;

        Assert.True((await Start(tableId)).IsSuccess);
        Assert.Equal(FloorErrors.PlayerAlreadyRated, (await Start(tableId)).Error);
    }

    [Fact]
    public async Task KK4_KK5_finish_computes_theoretical_win_and_checks_limits()
    {
        var tableId = (await Define("BJ-04")).Value; // min 50, max 5000, %2.5, 60 el/saat
        await Open(tableId);
        var ratingId = (await Start(tableId)).Value;
        _clock.UtcNow = T0.AddMinutes(90);

        var tooHigh = await Finish(ratingId, 6_000m);
        var ok = await Finish(ratingId, 200m, -1_000m);

        Assert.Equal(FloorErrors.BetOutsideLimits, tooHigh.Error);
        Assert.True(ok.IsSuccess);
        var rating = _ratings.Items.Single();
        Assert.Equal(450m, rating.TheoreticalWin); // 200 × 60 × 1.5 × 0.025
        Assert.Equal(-1_000m, rating.PlayerResult);
        Assert.Contains(rating.PendingChanges, c => c.Kind == FloorChangeKind.RatingFinished);
    }

    [Fact]
    public async Task KK6_correction_keeps_history_and_becomes_current_value()
    {
        var tableId = (await Define("BJ-05")).Value;
        await Open(tableId);
        var ratingId = (await Start(tableId)).Value;
        _clock.UtcNow = T0.AddHours(1);
        await Finish(ratingId, 100m);

        var result = await new CorrectRatingHandler(_tables, _ratings, _unitOfWork, _clock)
            .Handle(new CorrectRatingCommand(Tenant, ratingId, 300m, 500m, Audit()), default);

        var rating = _ratings.Items.Single();
        Assert.True(result.IsSuccess);
        Assert.Equal(300m, rating.AverageBet);
        Assert.Equal(450m, rating.TheoreticalWin); // 300 × 60 × 1 × 0.025
        var correction = Assert.Single(rating.Corrections);
        Assert.Equal(100m, correction.PreviousAverageBet);
    }

    [Fact]
    public async Task KK7_location_shows_running_rating_table()
    {
        var tableId = (await Define("RL-01", GameType.Roulette)).Value;
        await Open(tableId);
        var query = new PlayerLocationQuery(_ratings);

        Assert.Null(await query.CurrentLocation(Tenant, Player, default));
        await Start(tableId);

        var location = await query.CurrentLocation(Tenant, Player, default);
        Assert.Equal("RL-01", location!.TableCode);
        Assert.Equal(T0, location.Since);
    }

    [Fact]
    public async Task Running_ratings_are_listed_by_table()
    {
        var tableId = (await Define("BJ-09")).Value;
        await Open(tableId);
        var ratingId = (await Start(tableId)).Value;

        var running = await new ListRunningRatingsHandler(_ratings, _players).Handle(new ListRunningRatingsQuery(Tenant, tableId), default);

        Assert.Equal(ratingId, Assert.Single(running).RatingId);
        Assert.Equal("P000001", running[0].PlayerNumber);
        Assert.Empty(await new ListRunningRatingsHandler(_ratings, _players).Handle(new ListRunningRatingsQuery(Tenant, Guid.NewGuid()), default));
    }

    private Task<Result<Guid>> Define(string code, GameType type = GameType.Blackjack) =>
        new DefineTableHandler(_tables, _unitOfWork, _clock).Handle(
            new DefineTableCommand(Tenant, code, type, "TRY", 50m, 5_000m, 2.5m, 60, Audit()), default);

    private Task<Result> Open(Guid tableId) =>
        new OpenTableSessionHandler(_tables, _unitOfWork, _clock).Handle(new TableSessionCommand(Tenant, tableId, Audit()), default);

    private Task<Result> Close(Guid tableId) =>
        new CloseTableSessionHandler(_tables, _ratings, _unitOfWork, _clock).Handle(new TableSessionCommand(Tenant, tableId, Audit()), default);

    private Task<Result<Guid>> Start(Guid tableId) =>
        new StartRatingHandler(_tables, _ratings, _players, _unitOfWork, _clock).Handle(new StartRatingCommand(Tenant, tableId, Player, Audit()), default);

    private Task<Result> Finish(Guid ratingId, decimal averageBet, decimal? result = null) =>
        new FinishRatingHandler(_tables, _ratings, _unitOfWork, _clock).Handle(new FinishRatingCommand(Tenant, ratingId, averageBet, result, Audit()), default);

    private sealed class FakeTables : ITableRepository
    {
        public List<GamingTable> Items { get; } = [];

        public Task<GamingTable?> Get(TenantId tenantId, Guid tableId, CancellationToken ct) =>
            Task.FromResult(Items.SingleOrDefault(t => t.TenantId == tenantId && t.Id == tableId));

        public Task<bool> CodeExists(TenantId tenantId, string code, CancellationToken ct) =>
            Task.FromResult(Items.Any(t => t.TenantId == tenantId && t.Code == code));

        public Task<IReadOnlyList<GamingTable>> List(TenantId tenantId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<GamingTable>>(Items.Where(t => t.TenantId == tenantId).ToList());

        public void Add(GamingTable table) => Items.Add(table);
    }

    private sealed class FakeRatings : IRatingRepository
    {
        public List<Rating> Items { get; } = [];

        public Task<Rating?> Get(TenantId tenantId, Guid ratingId, CancellationToken ct) =>
            Task.FromResult(Items.SingleOrDefault(r => r.TenantId == tenantId && r.Id == ratingId));

        public Task<Rating?> RunningForPlayer(TenantId tenantId, Guid playerId, CancellationToken ct) =>
            Task.FromResult(Items.SingleOrDefault(r => r.TenantId == tenantId && r.PlayerId == playerId && r.IsRunning));

        public Task<int> RunningAtTable(TenantId tenantId, Guid tableId, CancellationToken ct) =>
            Task.FromResult(Items.Count(r => r.TenantId == tenantId && r.TableId == tableId && r.IsRunning));

        public Task<IReadOnlyList<Rating>> Running(TenantId tenantId, Guid? tableId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Rating>>(Items.Where(r => r.TenantId == tenantId && r.IsRunning && (tableId == null || r.TableId == tableId)).ToList());

        public void Add(Rating rating) => Items.Add(rating);
    }

    private sealed class FakePlayers : IPlayerStatusQuery
    {
        public bool Restricted { get; set; }

        public Task<PlayerStatusDto?> GetStatus(TenantId tenantId, Guid playerId, CancellationToken ct) =>
            Task.FromResult<PlayerStatusDto?>(new PlayerStatusDto(playerId, "P000001", Restricted));
    }

    private sealed class FakeUnitOfWork : IFloorUnitOfWork
    {
        public Task Commit(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class ManualClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }
}
