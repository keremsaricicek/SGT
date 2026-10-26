using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Finance.Application.Cage;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Tests;

public sealed class CageHandlerTests
{
    private readonly FakeShifts _shifts = new();
    private readonly FakePlayers _players = new();
    private readonly FakeRates _rates = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeClock _clock = new();

    [Fact]
    public async Task KK1_cashier_cannot_open_second_shift_and_terminal_is_exclusive()
    {
        Assert.True((await Open()).IsSuccess);
        Assert.Equal(FinanceErrors.ShiftAlreadyOpen, (await Open()).Error);
        Assert.Equal(FinanceErrors.ShiftAlreadyOpen, (await Open(CashierShiftTests.Audit(UserId.New()))).Error);
    }

    [Fact]
    public async Task KK5_same_idempotency_key_returns_first_result_without_second_movement()
    {
        var shiftId = (await Open()).Value;
        var key = Guid.NewGuid();

        var first = await SellChips(shiftId, key, 500m);
        var second = await SellChips(shiftId, key, 500m);
        var mismatch = await SellChips(shiftId, key, 600m);

        Assert.Equal(first.Value.TransactionId, second.Value.TransactionId);
        Assert.Equal(first.Value.Movements, second.Value.Movements);
        Assert.Equal(FinanceErrors.DuplicateKeyMismatch, mismatch.Error);
        Assert.Single(_shifts.Items.Single().Transactions);
        Assert.Equal(1, _unitOfWork.Commits - 1); // açılış + tek işlem
    }

    [Fact]
    public async Task KK6_restricted_or_unknown_player_cannot_transact()
    {
        var shiftId = (await Open()).Value;
        _players.Restricted = true;

        Assert.Equal(FinanceErrors.PlayerRestricted, (await SellChips(shiftId, Guid.NewGuid(), 10m)).Error);
        Assert.Equal(FinanceErrors.PlayerNotFound, (await SellChips(shiftId, Guid.NewGuid(), 10m, playerId: Guid.NewGuid())).Error);
    }

    [Fact]
    public async Task KK3_exchange_uses_current_rate_and_fails_without_rate()
    {
        var shiftId = (await Open()).Value;
        var handler = new ExchangeForeignToBaseHandler(new CageOperation(_shifts, _players, _unitOfWork, _clock), _rates);

        var noRate = await handler.Handle(new ExchangeCommand(CashierShiftTests.Tenant, shiftId, Guid.NewGuid(), CashierShiftTests.Player, 100m, "USD", CashierShiftTests.Audit()), default);
        _rates.Rate = new ExchangeRateDto("USD", 33m, 34m, CashierShiftTests.Now);
        var ok = await handler.Handle(new ExchangeCommand(CashierShiftTests.Tenant, shiftId, Guid.NewGuid(), CashierShiftTests.Player, 100m, "USD", CashierShiftTests.Audit()), default);

        Assert.Equal(FinanceErrors.RateUnavailable, noRate.Error);
        Assert.Equal(33m, ok.Value.Rate);
        Assert.Equal(10_000m - 3_300m, _shifts.Items.Single().Balance(Instrument.Cash, "TRY"));
    }

    [Fact]
    public async Task KK9_close_and_KK10_summary_show_expected_balances()
    {
        var shiftId = (await Open()).Value;
        await SellChips(shiftId, Guid.NewGuid(), 1_000m);

        var summary = await new GetShiftSummaryHandler(_shifts).Handle(new GetShiftSummaryQuery(CashierShiftTests.Tenant, shiftId), default);
        var close = await new CloseShiftHandler(_shifts, _unitOfWork, _clock).Handle(new CloseShiftCommand(
            CashierShiftTests.Tenant, shiftId,
            [new DrawerAmount(Instrument.Cash, "TRY", 11_000m), new DrawerAmount(Instrument.Chips, "TRY", 49_000m)], null, CashierShiftTests.Audit()), default);

        Assert.Contains(summary!.Balances, b => b.Instrument == "Cash" && b.Currency == "TRY" && b.Expected == 11_000m);
        Assert.Single(summary.Transactions);
        Assert.True(close.IsSuccess);
    }

    [Fact]
    public async Task My_open_shift_is_found_for_its_cashier_only()
    {
        var shiftId = (await Open()).Value;

        Assert.Equal(shiftId, await new GetMyOpenShiftHandler(_shifts).Handle(new GetMyOpenShiftQuery(CashierShiftTests.Tenant, CashierShiftTests.Cashier), default));
        Assert.Null(await new GetMyOpenShiftHandler(_shifts).Handle(new GetMyOpenShiftQuery(CashierShiftTests.Tenant, UserId.New()), default));
    }

    private Task<Result<Guid>> Open(AuditContext? audit = null) =>
        new OpenShiftHandler(_shifts, _unitOfWork, _clock).Handle(new OpenShiftCommand(
            CashierShiftTests.Tenant,
            [new DrawerAmount(Instrument.Cash, "TRY", 10_000m), new DrawerAmount(Instrument.Chips, "TRY", 50_000m)],
            audit ?? CashierShiftTests.Audit()), default);

    private Task<Result<CageTransactionResult>> SellChips(Guid shiftId, Guid key, decimal amount, Guid? playerId = null) =>
        new SellChipsHandler(new CageOperation(_shifts, _players, _unitOfWork, _clock)).Handle(new ChipCommand(
            CashierShiftTests.Tenant, shiftId, key, playerId ?? CashierShiftTests.Player, amount, "TRY", CashierShiftTests.Audit()), default);

    private sealed class FakeShifts : IShiftRepository
    {
        public List<CashierShift> Items { get; } = [];

        public Task<CashierShift?> Get(TenantId tenantId, Guid shiftId, CancellationToken ct) =>
            Task.FromResult(Items.SingleOrDefault(s => s.TenantId == tenantId && s.Id == shiftId));

        public Task<bool> HasOpenShift(TenantId tenantId, UserId cashier, string terminal, CancellationToken ct) =>
            Task.FromResult(Items.Any(s => s.TenantId == tenantId && s.IsOpen && (s.CashierId == cashier || s.Terminal == terminal)));

        public Task<TransactionLookup?> FindTransaction(TenantId tenantId, Guid transactionId, CancellationToken ct) =>
            Task.FromResult(Items.Where(s => s.TenantId == tenantId && s.FindTransaction(transactionId) is not null)
                .Select(s => new TransactionLookup(s.Id, s.FindTransaction(transactionId)!)).SingleOrDefault());

        public Task<Guid?> FindOpenShiftId(TenantId tenantId, UserId cashier, CancellationToken ct) =>
            Task.FromResult(Items.Where(s => s.TenantId == tenantId && s.IsOpen && s.CashierId == cashier).Select(s => (Guid?)s.Id).SingleOrDefault());

        public void Add(CashierShift shift) => Items.Add(shift);
    }

    private sealed class FakePlayers : IPlayerStatusQuery
    {
        public bool Restricted { get; set; }

        public Task<PlayerStatusDto?> GetStatus(TenantId tenantId, Guid playerId, CancellationToken ct) =>
            Task.FromResult(playerId == CashierShiftTests.Player ? new PlayerStatusDto(playerId, "P000001", Restricted) : null);
    }

    private sealed class FakeRates : IExchangeRateQuery
    {
        public ExchangeRateDto? Rate { get; set; }

        public Task<ExchangeRateDto?> RateAt(TenantId tenantId, string currency, DateTimeOffset at, CancellationToken ct) => Task.FromResult(Rate);
    }

    private sealed class FakeUnitOfWork : IFinanceUnitOfWork
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
        public DateTimeOffset UtcNow => CashierShiftTests.Now;
    }
}
