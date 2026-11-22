using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.ExchangeRates;

namespace Sgt.DemoData.Steps;

/// <summary>Orta kurdan alış/satış kuru üretip <see cref="SetExchangeRateCommand"/> ile yazar (Spec 004 kuralları aynen işler).</summary>
public sealed class RateBoard(DemoCommands commands)
{
    private const decimal Spread = 0.008m;

    /// <summary>Her yabancı para için kur girer; <paramref name="maxDrift"/> (ör. 0.006) kadar dalgalandırır.</summary>
    public async Task Publish(DemoRun run, decimal maxDrift, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        foreach (var currency in run.MidRates.Keys.Order(StringComparer.Ordinal).ToList())
        {
            var mid = decimal.Round(run.MidRates[currency] * (1 + ((run.NextRatio() * 2) - 1) * maxDrift), 4);
            run.MidRates[currency] = mid;
            var buy = decimal.Round(mid * (1 - Spread), 4);
            var sell = decimal.Round(mid * (1 + Spread), 4);
            await commands.Do<ICommandHandler<SetExchangeRateCommand>>(
                $"kur {currency}", h => h.Handle(new SetExchangeRateCommand(run.Tenant, currency, buy, sell, run.Audit(DemoUsers.SuperAdmin.Id)), ct));
        }
    }
}
