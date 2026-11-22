using System.Globalization;
using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Slots;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.DemoData.Steps;

/// <summary>
/// Gün sonu slot sayaç okuması (Spec 009 elle giriş komutuyla). Sayaçlar kümülatif artar; okuma zamanı komutta
/// açıkça verildiği için okumalar paralel yazılır. Değerler önce sırayla üretilir (determinizm, KK5).
/// </summary>
public sealed class SlotMeterRound(DemoCommands commands)
{
    private const int Parallelism = 8;

    public async Task Read(DemoRun run, DateTimeOffset readAt, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        var day = readAt.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var readings = run.Slots.Select(slot =>
        {
            var coinIn = decimal.Round(run.Next(1_500, 30_000) * (slot.Currency == SupportedCurrencies.Base ? 1m : 0.05m), 2);
            var payout = decimal.Round(coinIn * (0.86m + (run.NextRatio() * 0.09m)), 2);
            var jackpot = run.Chance(0.02) ? decimal.Round(coinIn * 0.3m, 2) : 0m;
            var games = (long)decimal.Ceiling(coinIn / (slot.Denomination * run.Next(10, 40)));
            slot.Play(coinIn, payout, jackpot, games);
            return new EnterMeterReadingCommand(
                run.Tenant, slot.AssetNumber, readAt, new MeterSnapshot(slot.CoinIn, slot.CoinOut, slot.Jackpot, slot.GamesPlayed),
                IsReset: false, Note: null, Reference: $"DEMO|{slot.AssetNumber}|{day}", run.Audit(DemoUsers.SlotManager.Id));
        }).ToList();

        await Parallel.ForEachAsync(
            readings,
            new ParallelOptions { MaxDegreeOfParallelism = Parallelism, CancellationToken = ct },
            async (command, token) => await commands.Do<ICommandHandler<EnterMeterReadingCommand>>("slot okuması", h => h.Handle(command, token)));
    }
}
