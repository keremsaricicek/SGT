using System.Globalization;
using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Domain;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Application.Slots;

public sealed record DefineSlotMachineCommand(
    TenantId TenantId, string AssetNumber, string Area, string Manufacturer, string Model, decimal Denomination, string Currency, AuditContext Audit);

/// <summary>Makine tanımı (Spec 009 KK1).</summary>
public sealed class DefineSlotMachineHandler(ISlotMachineRepository machines, IFloorUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DefineSlotMachineCommand, Guid>
{
    public async Task<Result<Guid>> Handle(DefineSlotMachineCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var machine = SlotMachine.Define(
            Guid.NewGuid(), command.TenantId, command.AssetNumber, command.Area, command.Manufacturer, command.Model, command.Denomination,
            command.Currency, command.Audit, clock.UtcNow);
        if (machine.IsFailure)
        {
            return machine.Error;
        }

        if (await machines.AssetExists(command.TenantId, machine.Value.AssetNumber, ct))
        {
            return SlotErrors.AssetNumberTaken;
        }

        machines.Add(machine.Value);
        await unitOfWork.Commit(ct);
        return machine.Value.Id;
    }
}

public sealed record EnterMeterReadingCommand(
    TenantId TenantId, string AssetNumber, DateTimeOffset ReadAt, MeterSnapshot Meters, bool IsReset, string? Note, string Reference, AuditContext Audit);

/// <summary>Elle sayaç girişi (Spec 009 KK6).</summary>
public sealed class EnterMeterReadingHandler(MeterReadingRecorder recorder) : ICommandHandler<EnterMeterReadingCommand>
{
    public Task<Result> Handle(EnterMeterReadingCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return recorder.Record(
            command.TenantId,
            new MeterReadingInput(command.AssetNumber, command.ReadAt, command.Meters, ReadingSource.Manual, command.Reference, command.IsReset, command.Note),
            command.Audit,
            ct,
            FloorChangeKind.SlotReadingEntered);
    }
}

public sealed record ImportMeterReadingsCommand(TenantId TenantId, string Csv, AuditContext Audit);

public sealed record ImportLineError(int Line, string Code, string Message);

public sealed record ImportReport(int Imported, IReadOnlyList<ImportLineError> Errors);

/// <summary>
/// CSV içe aktarma (Spec 009 KK6). Sütunlar: asset_number,read_at,coin_in,coin_out,jackpot,games_played[,reference].
/// Referans yoksa "varlık|zaman" kullanılır; aynı dosya iki kez yüklenirse ikinci yükleme yeni okuma üretmez.
/// </summary>
public sealed class ImportMeterReadingsHandler(MeterReadingRecorder recorder, IFloorEvents events, IFloorUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ImportMeterReadingsCommand, ImportReport>
{
    private static readonly Error InvalidRow = new("Slot.InvalidCsvRow", "Satır biçimi geçersiz.");

    public async Task<Result<ImportReport>> Handle(ImportMeterReadingsCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var lines = command.Csv.Split('\n').Select(l => l.Trim()).ToList();
        var imported = 0;
        var errors = new List<ImportLineError>();

        for (var i = 1; i < lines.Count; i++)
        {
            if (lines[i].Length == 0)
            {
                continue;
            }

            var lineNumber = i + 1;
            var input = Parse(lines[i]);
            var result = input is null ? InvalidRow : await recorder.Record(command.TenantId, input, command.Audit, ct);
            if (result.IsSuccess)
            {
                imported++;
            }
            else
            {
                errors.Add(new ImportLineError(lineNumber, result.Error.Code, result.Error.Message));
            }
        }

        RecordSummary(command, imported, errors.Count);
        await unitOfWork.Commit(ct);
        return new ImportReport(imported, errors);
    }

    private static MeterReadingInput? Parse(string line)
    {
        var c = line.Split(',').Select(x => x.Trim()).ToArray();
        var ok = c.Length is 6 or 7
            & DateTimeOffset.TryParse(c[1], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var readAt)
            & decimal.TryParse(c[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var coinIn)
            & decimal.TryParse(c.ElementAtOrDefault(3), NumberStyles.Number, CultureInfo.InvariantCulture, out var coinOut)
            & decimal.TryParse(c.ElementAtOrDefault(4), NumberStyles.Number, CultureInfo.InvariantCulture, out var jackpot)
            & long.TryParse(c.ElementAtOrDefault(5), NumberStyles.Integer, CultureInfo.InvariantCulture, out var games);
        if (!ok)
        {
            return null;
        }

        var reference = c.Length == 7 && c[6].Length > 0 ? c[6] : $"{c[0].ToUpperInvariant()}|{readAt.UtcDateTime:O}";
        return new MeterReadingInput(c[0], readAt, new MeterSnapshot(coinIn, coinOut, jackpot, games), ReadingSource.Import, reference, false, null);
    }

    private void RecordSummary(ImportMeterReadingsCommand command, int imported, int failed) =>
        events.Publish(command.TenantId, new FloorChange(
            Guid.NewGuid(), FloorChangeKind.SlotReadingsImported, $"{imported} okuma, {failed} hata", command.Audit, clock.UtcNow));
}

public sealed record GetSlotFloorQuery(TenantId TenantId);

public sealed record SlotFloorRow(
    Guid MachineId, string AssetNumber, string Area, string Currency, DateTimeOffset? LastReadAt, decimal TodayCoinIn, decimal TodayWin);

/// <summary>Slot salonu (Spec 009 KK7): iş günü (KKTC) toplamları okuma farklarından hesaplanır.</summary>
public sealed class GetSlotFloorHandler(ISlotMachineRepository machines, IClock clock) : IQueryHandler<GetSlotFloorQuery, IReadOnlyList<SlotFloorRow>>
{
    public async Task<IReadOnlyList<SlotFloorRow>> Handle(GetSlotFloorQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var day = BusinessDay.For(clock.LocalToday());
        var readings = await machines.ReadingsBetween(query.TenantId, day.Start, day.End, ct);
        var byMachine = readings.GroupBy(r => r.MachineId).ToDictionary(g => g.Key, g => g.Aggregate(MeterSnapshot.Zero, (sum, r) => sum.Plus(r.Delta)));

        return (await machines.List(query.TenantId, ct))
            .OrderBy(m => m.AssetNumber, StringComparer.Ordinal)
            .Select(m =>
            {
                var totals = byMachine.GetValueOrDefault(m.Id, MeterSnapshot.Zero);
                return new SlotFloorRow(m.Id, m.AssetNumber, m.Area, m.Currency, m.LastReadAt, totals.CoinIn, totals.MachineWin);
            })
            .ToList();
    }
}

public sealed record GetSlotMachineQuery(TenantId TenantId, Guid MachineId);

public sealed record ReadingView(DateTimeOffset ReadAt, string Source, MeterSnapshot Meters, MeterSnapshot Delta, decimal DeltaWin, bool IsReset, string? Note);

public sealed record SlotMachineView(
    Guid MachineId, string AssetNumber, string Area, string Manufacturer, string Model, decimal Denomination, string Currency, IReadOnlyList<ReadingView> Readings);

/// <summary>Makine detayı ve son okumalar (Spec 009 KK7).</summary>
public sealed class GetSlotMachineHandler(ISlotMachineRepository machines) : IQueryHandler<GetSlotMachineQuery, SlotMachineView?>
{
    private const int RecentReadings = 50;

    public async Task<SlotMachineView?> Handle(GetSlotMachineQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var machine = await machines.Get(query.TenantId, query.MachineId, ct);
        if (machine is null)
        {
            return null;
        }

        var readings = await machines.LatestReadings(query.TenantId, machine.Id, RecentReadings, ct);
        return new SlotMachineView(
            machine.Id, machine.AssetNumber, machine.Area, machine.Manufacturer, machine.Model, machine.Denomination, machine.Currency,
            readings.Select(r => new ReadingView(r.Reading.ReadAt, r.Reading.Source.ToString(), r.Reading.Meters, r.Delta, r.Delta.MachineWin, r.Reading.IsReset, r.Reading.Note))
                .ToList());
    }
}
