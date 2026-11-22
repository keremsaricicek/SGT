using System.Globalization;
using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Slots;
using Sgt.Modules.GamingFloor.Application.Tables;
using Sgt.Modules.GamingFloor.Domain;

namespace Sgt.DemoData.Steps;

/// <summary>Oyun salonu (Spec 012 KK3): canlı oyun masaları (TRY ağırlıklı, bir kısmı EUR) ve slot makineleri.</summary>
public sealed class FloorSeeder(DemoCommands commands) : IDemoStep
{
    private const int EuroTableEvery = 5;
    private const int EuroSlotEvery = 10;

    private static readonly TableKind[] Kinds =
    [
        new("RL", GameType.Roulette, 2.70m, 35, TryMin: 50, EurMin: 5),
        new("BJ", GameType.Blackjack, 1.50m, 60, TryMin: 100, EurMin: 10),
        new("PK", GameType.Poker, 2.50m, 30, TryMin: 200, EurMin: 20),
        new("BC", GameType.Baccarat, 1.20m, 70, TryMin: 500, EurMin: 50),
    ];

    private static readonly string[] Areas = ["A", "B", "C", "VIP"];
    private static readonly string[] Manufacturers = ["Demo Gaming", "Atlas Slot", "Kuzey Oyun"];
    private static readonly decimal[] Denominations = [0.01m, 0.05m, 0.10m, 0.25m, 1.00m];

    public int Order => 50;

    public string Name => "Masalar ve slotlar";

    public async Task Run(DemoRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        for (var i = 0; i < run.Scale.Tables; i++)
        {
            var kind = Kinds[i % Kinds.Length];
            var currency = (i + 1) % EuroTableEvery == 0 ? "EUR" : SupportedCurrencies.Base;
            var min = currency == SupportedCurrencies.Base ? kind.TryMin : kind.EurMin;
            var code = string.Create(CultureInfo.InvariantCulture, $"{kind.Prefix}-{(i / Kinds.Length) + 1:D2}");
            var command = new DefineTableCommand(
                run.Tenant, code, kind.GameType, currency, min, min * 50, kind.HouseEdgePercent, kind.HandsPerHour, run.Audit(DemoUsers.CasinoManager.Id));
            var id = await commands.Ask<ICommandHandler<DefineTableCommand, Guid>, Guid>("masa", h => h.Handle(command, ct));
            run.Tables.Add(new DemoTable(id, code, currency, min, min * 50));
        }

        for (var i = 1; i <= run.Scale.SlotMachines; i++)
        {
            var asset = "SL-" + i.ToString("D4", CultureInfo.InvariantCulture);
            var currency = i % EuroSlotEvery == 0 ? "EUR" : SupportedCurrencies.Base;
            var denomination = run.Pick(Denominations);
            var command = new DefineSlotMachineCommand(
                run.Tenant, asset, run.Pick(Areas), run.Pick(Manufacturers), "Model " + run.Next(1, 9).ToString(CultureInfo.InvariantCulture),
                denomination, currency, run.Audit(DemoUsers.SlotManager.Id));
            await commands.Ask<ICommandHandler<DefineSlotMachineCommand, Guid>, Guid>("slot makinesi", h => h.Handle(command, ct));
            run.Slots.Add(new DemoSlot(asset, currency, denomination));
        }
    }

    private sealed record TableKind(string Prefix, GameType GameType, decimal HouseEdgePercent, int HandsPerHour, decimal TryMin, decimal EurMin);
}
