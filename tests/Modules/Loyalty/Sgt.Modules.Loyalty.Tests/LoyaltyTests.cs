using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.GamingFloor.Contracts;
using Sgt.Modules.Loyalty.Application.Comps;
using Sgt.Modules.Loyalty.Application.Earning;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Tests;

public sealed class LoyaltyTests
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    private static readonly Guid Player = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 22, 0, 0, TimeSpan.Zero);

    private readonly FakeAccounts _accounts = new();
    private readonly FakeRequests _requests = new();
    private readonly FakeInbox _inbox = new();
    private readonly FakeRates _rates = new();
    private readonly FakePlayers _players = new();
    private readonly FakeRules _rules = new();
    private readonly FakeUnitOfWork _unitOfWork;

    public LoyaltyTests()
    {
        _unitOfWork = new FakeUnitOfWork(_inbox);
    }

    private static AuditContext Audit(UserId? actor = null) => new(actor ?? UserId.New(), "HOST-1", ReasonCode.Of("LOYALTY"), Guid.NewGuid());

    [Fact]
    public void KK4_tier_follows_lifetime_earned_points()
    {
        Assert.Equal(Tier.Bronze, Tiers.For(999));
        Assert.Equal(Tier.Silver, Tiers.For(1_000));
        Assert.Equal(Tier.Gold, Tiers.For(5_000));
        Assert.Equal(Tier.Platinum, Tiers.For(20_000));
    }

    [Fact]
    public async Task KK1_rating_earns_points_by_rule_and_KK2_same_event_twice_earns_once()
    {
        var e = Rated(theoreticalWin: 12_345m);

        await Earn(e);
        await Earn(e);

        var account = _accounts.Items.Single();
        Assert.Equal(123, account.Balance);
        Assert.Equal(123, account.LifetimeEarned);
    }

    [Fact]
    public async Task KK1_foreign_currency_rating_is_converted_at_buy_rate_and_waits_for_rate()
    {
        var e = Rated(theoreticalWin: 100m, currency: "EUR");

        await Assert.ThrowsAsync<InvalidOperationException>(() => Earn(e));
        _rates.Rate = new ExchangeRateDto("EUR", 36m, 37m, Now.AddDays(-1));
        await Earn(e);

        Assert.Equal(36, _accounts.Items.Single().Balance); // 100 × 36 / 100
    }

    [Fact]
    public async Task KK2_correction_records_only_the_difference()
    {
        var ratingId = Guid.NewGuid();
        await Earn(Rated(theoreticalWin: 1_000m, ratingId: ratingId));
        await Earn(Rated(theoreticalWin: 600m, ratingId: ratingId, isCorrection: true));

        var account = _accounts.Items.Single();
        Assert.Equal(6, account.Balance);
        Assert.Equal([10, -4], account.Entries.Select(x => x.Points));
    }

    [Fact]
    public async Task KK1_rule_rate_can_be_changed()
    {
        _rules.TryPerPoint = 10m;

        await Earn(Rated(theoreticalWin: 55m));

        Assert.Equal(5, _accounts.Items.Single().Balance);
    }

    [Fact]
    public async Task KK6_comp_needs_a_second_person_and_enough_points()
    {
        await Earn(Rated(theoreticalWin: 50_000m)); // 500 puan
        var host = UserId.New();
        var request = await new RequestCompHandler(_requests, _players, _unitOfWork, new FixedClock())
            .Handle(new RequestCompCommand(Tenant, Guid.NewGuid(), Player, 300, "Akşam yemeği", Audit(host)), default);
        var tooMuch = await new RequestCompHandler(_requests, _players, _unitOfWork, new FixedClock())
            .Handle(new RequestCompCommand(Tenant, Guid.NewGuid(), Player, 900, "Oda", Audit(host)), default);
        var approve = new DecideCompHandler(_requests, _accounts, _players, _unitOfWork, new FixedClock());

        var self = await approve.Handle(new DecideCompCommand(Tenant, request.Value, Approve: true, Audit(host)), default);
        var other = await approve.Handle(new DecideCompCommand(Tenant, request.Value, Approve: true, Audit()), default);
        var insufficient = await approve.Handle(new DecideCompCommand(Tenant, tooMuch.Value, Approve: true, Audit()), default);

        Assert.Equal(LoyaltyErrors.FourEyesRequired, self.Error);
        Assert.True(other.IsSuccess);
        Assert.Equal(LoyaltyErrors.InsufficientPoints, insufficient.Error);
        Assert.Equal(200, _accounts.Items.Single().Balance);
    }

    [Fact]
    public async Task Same_request_id_does_not_open_second_comp()
    {
        var id = Guid.NewGuid();
        var handler = new RequestCompHandler(_requests, _players, _unitOfWork, new FixedClock());

        var first = await handler.Handle(new RequestCompCommand(Tenant, id, Player, 5, "Bar", Audit()), default);
        var again = await handler.Handle(new RequestCompCommand(Tenant, id, Player, 5, "Bar", Audit()), default);
        var other = await handler.Handle(new RequestCompCommand(Tenant, id, Player, 6, "Bar", Audit()), default);

        Assert.Equal(first.Value, again.Value);
        Assert.Equal(LoyaltyErrors.DuplicateRequestMismatch, other.Error);
    }

    [Fact]
    public async Task KK6_restricted_player_cannot_request_comp()
    {
        _players.Restricted = true;

        var result = await new RequestCompHandler(_requests, _players, _unitOfWork, new FixedClock())
            .Handle(new RequestCompCommand(Tenant, Guid.NewGuid(), Player, 10, "Bar", Audit()), default);

        Assert.Equal(LoyaltyErrors.PlayerRestricted, result.Error);
    }

    [Fact]
    public void KK3_KK5_negative_adjustment_cannot_drop_balance_below_zero()
    {
        var account = LoyaltyAccount.Open(Tenant, Player);

        Assert.Equal(LoyaltyErrors.InsufficientPoints, account.Adjust(Guid.NewGuid(), -1, Audit(), Now).Error);
        Assert.True(account.Adjust(Guid.NewGuid(), 50, Audit(), Now).IsSuccess);
        Assert.Equal(50, account.Balance);
        Assert.Equal(0, account.LifetimeEarned);
    }

    private Task Earn(PlayerRatedV1 e) =>
        new PlayerRatedHandler(_accounts, _rules, _rates, _inbox, _unitOfWork).Handle(e, default);

    private static PlayerRatedV1 Rated(decimal theoreticalWin, string currency = "TRY", Guid? ratingId = null, bool isCorrection = false) => new(
        Guid.NewGuid(), Guid.NewGuid(), Now, Tenant, Audit(), ratingId ?? Guid.NewGuid(), Player, "BJ-01", currency, 100m, null, theoreticalWin,
        Now.AddHours(-1), Now, isCorrection);

    private sealed class FakeAccounts : ILoyaltyAccountRepository
    {
        public List<LoyaltyAccount> Items { get; } = [];

        public Task<LoyaltyAccount?> Get(TenantId tenantId, Guid playerId, CancellationToken ct) =>
            Task.FromResult(Items.SingleOrDefault(a => a.TenantId == tenantId && a.PlayerId == playerId));

        public void Add(LoyaltyAccount account) => Items.Add(account);
    }

    private sealed class FakeRequests : IApprovalRequestRepository
    {
        private readonly List<CompRequest> _comps = [];

        public Task<CompRequest?> GetComp(TenantId tenantId, Guid requestId, CancellationToken ct) =>
            Task.FromResult(_comps.SingleOrDefault(r => r.TenantId == tenantId && r.Id == requestId));

        public Task<PointsAdjustmentRequest?> GetAdjustment(TenantId tenantId, Guid requestId, CancellationToken ct) =>
            Task.FromResult<PointsAdjustmentRequest?>(null);

        public void Add(CompRequest request) => _comps.Add(request);

        public void Add(PointsAdjustmentRequest request)
        {
        }
    }

    private sealed class FakeInbox : ILoyaltyInbox
    {
        private readonly HashSet<(Guid, string)> _pending = [];
        private readonly HashSet<(Guid, string)> _done = [];

        public Task<bool> AlreadyProcessed(Guid eventId, string handler, CancellationToken ct) => Task.FromResult(_done.Contains((eventId, handler)));

        public void MarkProcessed(Guid eventId, string handler) => _pending.Add((eventId, handler));

        public void Commit()
        {
            _done.UnionWith(_pending);
            _pending.Clear();
        }
    }

    private sealed class FakeRates : IExchangeRateQuery
    {
        public ExchangeRateDto? Rate { get; set; }

        public Task<ExchangeRateDto?> RateAt(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct) => Task.FromResult(Rate);
    }

    private sealed class FakePlayers : IPlayerStatusQuery
    {
        public bool Restricted { get; set; }

        public Task<PlayerStatusDto?> GetStatus(TenantId tenantId, Guid playerId, CancellationToken ct) =>
            Task.FromResult<PlayerStatusDto?>(new PlayerStatusDto(playerId, "P000001", Restricted));
    }

    private sealed class FakeRules : IEarningRuleStore
    {
        public decimal TryPerPoint { get; set; } = EarningRule.DefaultTryPerPoint;

        public Task<EarningRule> Current(TenantId tenantId, CancellationToken ct) => Task.FromResult(EarningRule.Restore(tenantId, TryPerPoint));

        public void Save(EarningRule rule, AuditContext audit) => TryPerPoint = rule.TryPerPoint;
    }

    private sealed class FakeUnitOfWork(FakeInbox inbox) : ILoyaltyUnitOfWork
    {
        public Task Commit(CancellationToken ct)
        {
            inbox.Commit();
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => Now;
    }
}
