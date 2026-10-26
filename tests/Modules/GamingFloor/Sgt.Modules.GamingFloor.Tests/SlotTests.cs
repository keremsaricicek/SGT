using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Application.Slots;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Tests;

public sealed class SlotTests
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    private static readonly DateTimeOffset T0 = new(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

    private readonly FakeMachines _machines = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeEvents _events = new();
    private readonly ManualClock _clock = new() { UtcNow = T0.AddHours(12) };

    private static AuditContext Audit() => new(UserId.New(), "SLOT-1", ReasonCode.Of("METERS"), Guid.NewGuid());

    private static MeterSnapshot Meters(decimal coinIn, decimal coinOut, decimal jackpot = 0, long games = 0) => new(coinIn, coinOut, jackpot, games);

    [Fact]
    public void KK1_machine_definition_is_validated()
    {
        Assert.True(SlotMachine.Define(Guid.NewGuid(), Tenant, "A-1001", "Salon 1", "IGT", "Model X", 0.01m, "TRY", Audit(), T0).IsSuccess);
        Assert.True(SlotMachine.Define(Guid.NewGuid(), Tenant, "", "Salon 1", "IGT", "X", 0.01m, "TRY", Audit(), T0).IsFailure);
        Assert.True(SlotMachine.Define(Guid.NewGuid(), Tenant, "A-1", "Salon 1", "IGT", "X", 0m, "TRY", Audit(), T0).IsFailure);
    }

    [Fact]
    public void KK3_KK5_readings_must_grow_and_produce_deltas()
    {
        var machine = NewMachine();
        var first = machine.Record(Reading(T0, Meters(1_000m, 800m, 0, 100))).Value;
        var second = machine.Record(Reading(T0.AddHours(1), Meters(1_500m, 1_100m, 50m, 160))).Value;

        Assert.Equal(Meters(1_000m, 800m, 0, 100), first.Delta); // ilk okuma: değerin kendisi
        Assert.Equal(Meters(500m, 300m, 50m, 60), second.Delta);
        Assert.Equal(150m, second.Delta.MachineWin);

        Assert.Equal(SlotErrors.MeterWentBackwards, machine.Record(Reading(T0.AddHours(2), Meters(1_400m, 1_100m, 50m, 160))).Error);
        Assert.Equal(SlotErrors.ReadingNotAfterPrevious, machine.Record(Reading(T0.AddHours(1), Meters(2_000m, 1_100m, 50m, 160))).Error);
    }

    [Fact]
    public void KK3_reset_needs_note_and_delta_restarts_from_new_value()
    {
        var machine = NewMachine();
        machine.Record(Reading(T0, Meters(1_000m, 800m)));

        Assert.Equal(SlotErrors.ResetNoteRequired, machine.Record(Reading(T0.AddHours(1), Meters(10m, 5m), reset: true)).Error);
        var afterReset = machine.Record(Reading(T0.AddHours(1), Meters(10m, 5m), reset: true, note: "RAM clear")).Value;

        Assert.Equal(Meters(10m, 5m), afterReset.Delta);
    }

    [Fact]
    public async Task KK4_same_source_reference_is_recorded_once()
    {
        _machines.Add(NewMachine());
        var recorder = new MeterReadingRecorder(_machines, _events, _unitOfWork);
        var input = new MeterReadingInput("A-1001", T0, Meters(100m, 50m), ReadingSource.Import, "file1:2", false, null);

        Assert.True((await recorder.Record(Tenant, input, Audit(), default)).IsSuccess);
        Assert.True((await recorder.Record(Tenant, input, Audit(), default)).IsSuccess);
        Assert.Single(_machines.Readings);
    }

    [Fact]
    public async Task KK6_csv_import_reports_each_row()
    {
        _machines.Add(NewMachine());
        const string Csv = """
            asset_number,read_at,coin_in,coin_out,jackpot,games_played
            A-1001,2026-10-26T08:00:00Z,1000.00,800.00,0,100
            A-9999,2026-10-26T08:00:00Z,1,1,0,1
            A-1001,2026-10-26T09:00:00Z,900,900,0,100
            A-1001,bozuk,1,1,0,1
            A-1001,2026-10-26T10:00:00Z,1200.50,900,0,130
            """;

        var report = await new ImportMeterReadingsHandler(new MeterReadingRecorder(_machines, _events, _unitOfWork), _events, _unitOfWork, _clock)
            .Handle(new ImportMeterReadingsCommand(Tenant, Csv, Audit()), default);

        Assert.Equal(2, report.Value.Imported);
        Assert.Equal([3, 4, 5], report.Value.Errors.Select(e => e.Line));
        Assert.Equal(2, _machines.Readings.Count);
    }

    [Fact]
    public async Task KK7_floor_shows_todays_totals_in_local_business_day()
    {
        var machine = NewMachine();
        _machines.Add(machine);
        var recorder = new MeterReadingRecorder(_machines, _events, _unitOfWork);
        await recorder.Record(Tenant, new MeterReadingInput("A-1001", T0.AddDays(-1), Meters(500m, 400m), ReadingSource.Manual, "r1", false, null), Audit(), default);
        await recorder.Record(Tenant, new MeterReadingInput("A-1001", T0, Meters(1_500m, 1_000m, 100m), ReadingSource.Manual, "r2", false, null), Audit(), default);

        var floor = await new GetSlotFloorHandler(_machines, _clock).Handle(new GetSlotFloorQuery(Tenant), default);

        var row = Assert.Single(floor);
        Assert.Equal(1_000m, row.TodayCoinIn);
        Assert.Equal(300m, row.TodayWin); // 1000 − 600 − 100
        Assert.Equal(T0, row.LastReadAt);
    }

    private static SlotMachine NewMachine() =>
        SlotMachine.Define(Guid.NewGuid(), Tenant, "A-1001", "Salon 1", "IGT", "Model X", 0.01m, "TRY", Audit(), T0).Value;

    private static MeterReading Reading(DateTimeOffset at, MeterSnapshot meters, bool reset = false, string? note = null) =>
        new(Guid.NewGuid(), at, meters, ReadingSource.Manual, Guid.NewGuid().ToString(), reset, note, Audit());

    private sealed class FakeMachines : ISlotMachineRepository
    {
        private readonly List<SlotMachine> _items = [];

        public List<RecordedReading> Readings { get; } = [];

        public Task<SlotMachine?> GetByAsset(TenantId tenantId, string assetNumber, CancellationToken ct) =>
            Task.FromResult(_items.SingleOrDefault(m => m.TenantId == tenantId && string.Equals(m.AssetNumber, assetNumber.Trim(), StringComparison.OrdinalIgnoreCase)));

        public Task<SlotMachine?> Get(TenantId tenantId, Guid machineId, CancellationToken ct) =>
            Task.FromResult(_items.SingleOrDefault(m => m.TenantId == tenantId && m.Id == machineId));

        public Task<bool> AssetExists(TenantId tenantId, string assetNumber, CancellationToken ct) =>
            Task.FromResult(_items.Any(m => m.TenantId == tenantId && m.AssetNumber == assetNumber));

        public Task<bool> ReadingExists(TenantId tenantId, ReadingSource source, string reference, CancellationToken ct) =>
            Task.FromResult(Readings.Any(r => r.Reading.Source == source && r.Reading.SourceReference == reference));

        public Task<IReadOnlyList<SlotMachine>> List(TenantId tenantId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<SlotMachine>>(_items.Where(m => m.TenantId == tenantId).ToList());

        public Task<IReadOnlyList<RecordedReading>> ReadingsBetween(TenantId tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<RecordedReading>>(Readings.Where(r => r.Reading.ReadAt >= from && r.Reading.ReadAt < to).ToList());

        public Task<IReadOnlyList<RecordedReading>> LatestReadings(TenantId tenantId, Guid machineId, int take, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<RecordedReading>>(Readings.Where(r => r.MachineId == machineId).OrderByDescending(r => r.Reading.ReadAt).Take(take).ToList());

        public void Add(SlotMachine machine) => _items.Add(machine);

        public void AddReading(RecordedReading reading) => Readings.Add(reading);
    }

    private sealed class FakeEvents : IFloorEvents
    {
        public List<Sgt.Modules.GamingFloor.Domain.FloorChange> Published { get; } = [];

        public void Publish(TenantId tenantId, Sgt.Modules.GamingFloor.Domain.FloorChange change) => Published.Add(change);
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
