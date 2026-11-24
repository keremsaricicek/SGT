using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Cards;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Application.Profile;
using Sgt.Modules.CasinoPlayer.Application.Restrictions;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Tests;

public sealed class PlayerHandlerTests
{
    private readonly FakePlayers _players = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeClock _clock = new();

    [Fact]
    public async Task KK3_players_get_increasing_numbers()
    {
        var first = await Register(PlayerTests.Details(document: "A1"));
        var second = await Register(PlayerTests.Details(document: "A2"));

        Assert.Equal("P000001", first.Value.PlayerNumber);
        Assert.Equal("P000002", second.Value.PlayerNumber);
        Assert.Equal(2, _unitOfWork.Commits);
    }

    [Fact]
    public async Task KK2_same_document_cannot_register_twice()
    {
        await Register(PlayerTests.Details(document: "X-1"));

        Assert.Equal(PlayerErrors.DocumentInUse, (await Register(PlayerTests.Details(document: "x1"))).Error);
    }

    [Fact]
    public async Task KK6_update_cannot_take_another_players_document()
    {
        await Register(PlayerTests.Details(document: "D1"));
        var second = await Register(PlayerTests.Details(document: "D2"));

        var result = await new UpdatePlayerDetailsHandler(_players, _unitOfWork, _clock).Handle(
            new UpdatePlayerDetailsCommand(PlayerTests.Tenant, new PlayerId(second.Value.PlayerId), PlayerTests.Details(document: "D1"), PlayerTests.Audit()), default);
        var ownDocument = await new UpdatePlayerDetailsHandler(_players, _unitOfWork, _clock).Handle(
            new UpdatePlayerDetailsCommand(PlayerTests.Tenant, new PlayerId(second.Value.PlayerId), PlayerTests.Details(document: "D2") with { Phone = null }, PlayerTests.Audit()), default);

        Assert.Equal(PlayerErrors.DocumentInUse, result.Error);
        Assert.True(ownDocument.IsSuccess);
    }

    [Fact]
    public async Task KK7_card_number_is_unique_across_players()
    {
        var first = await Register(PlayerTests.Details(document: "C1"));
        var second = await Register(PlayerTests.Details(document: "C2"));
        var issue = new IssuePlayerCardHandler(_players, _unitOfWork, _clock);

        var ok = await issue.Handle(new IssuePlayerCardCommand(PlayerTests.Tenant, new PlayerId(first.Value.PlayerId), "777001", PlayerTests.Audit()), default);
        var taken = await issue.Handle(new IssuePlayerCardCommand(PlayerTests.Tenant, new PlayerId(second.Value.PlayerId), "777001", PlayerTests.Audit()), default);

        Assert.True(ok.IsSuccess);
        Assert.Equal(PlayerErrors.CardInUse, taken.Error);
    }

    [Fact]
    public async Task KK10_status_query_reports_restriction()
    {
        var registered = await Register(PlayerTests.Details(document: "R1"));
        var id = new PlayerId(registered.Value.PlayerId);
        var status = new PlayerStatusQuery(_players, _clock);

        Assert.False((await status.GetStatus(PlayerTests.Tenant, id.Value, default))!.IsRestricted);
        await new AddPlayerRestrictionHandler(_players, _unitOfWork, _clock).Handle(
            new AddPlayerRestrictionCommand(PlayerTests.Tenant, id, RestrictionType.Banned, "kural ihlali", null, PlayerTests.Audit()), default);

        var after = await status.GetStatus(PlayerTests.Tenant, id.Value, default);
        Assert.True(after!.IsRestricted);
        Assert.Equal("P000001", after.PlayerNumber);
        Assert.Null(await status.GetStatus(PlayerTests.Tenant, Guid.NewGuid(), default));
    }

    [Fact]
    public async Task KK4_player_view_masks_document()
    {
        var registered = await Register(PlayerTests.Details(document: "PASS998877"));

        var view = await new GetPlayerHandler(_players, _clock).Handle(new GetPlayerQuery(PlayerTests.Tenant, new PlayerId(registered.Value.PlayerId)), default);

        Assert.Equal("****8877", view!.MaskedDocumentNumber);
    }

    private Task<Result<PlayerRegistered>> Register(PlayerDetails details) =>
        new RegisterPlayerHandler(_players, _unitOfWork, _clock).Handle(new RegisterPlayerCommand(PlayerTests.Tenant, details, PlayerTests.Audit()), default);

    private sealed class FakePlayers : IPlayerRepository
    {
        private readonly List<Player> _items = [];

        public Task<Player?> Get(TenantId tenantId, PlayerId playerId, CancellationToken ct) =>
            Task.FromResult(_items.SingleOrDefault(p => p.TenantId == tenantId && p.Id == playerId));

        public Task<long> NextSequence(TenantId tenantId, CancellationToken ct) =>
            Task.FromResult(_items.Count(p => p.TenantId == tenantId) + 1L);

        public Task<bool> DocumentInUse(TenantId tenantId, DocumentType type, string number, PlayerId? except, CancellationToken ct) =>
            Task.FromResult(_items.Any(p => p.TenantId == tenantId && p.Id != except && p.Details.DocumentType == type && p.Details.DocumentNumber == number));

        public Task<bool> CardInUse(TenantId tenantId, CardNumber number, CancellationToken ct) =>
            Task.FromResult(_items.Any(p => p.TenantId == tenantId && p.Cards.Any(c => c.Number == number)));

        public Task<IReadOnlyList<PlayerSummary>> Search(TenantId tenantId, string term, int take, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<PlayerSummary>>([]);

        public void Add(Player player) => _items.Add(player);
    }

    private sealed class FakeUnitOfWork : IPlayerUnitOfWork
    {
        public int Commits { get; private set; }

        public Task Commit(CancellationToken ct)
        {
            Commits++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow => PlayerTests.Now;
    }
}
