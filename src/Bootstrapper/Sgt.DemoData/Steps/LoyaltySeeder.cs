using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Application.Rules;

namespace Sgt.DemoData.Steps;

/// <summary>Puan kazanma kuralı (Spec 012 KK3): teorik kazancın her 20 TRY'si 1 puan.</summary>
public sealed class LoyaltySeeder(DemoCommands commands) : IDemoStep
{
    public const decimal TryPerPoint = 20m;

    public int Order => 30;

    public string Name => "Puan kuralı";

    public Task Run(DemoRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        return commands.Do<ICommandHandler<SetEarningRuleCommand>>(
            "puan kuralı", h => h.Handle(new SetEarningRuleCommand(run.Tenant, TryPerPoint, run.Audit(DemoUsers.CasinoManager.Id)), ct));
    }
}
