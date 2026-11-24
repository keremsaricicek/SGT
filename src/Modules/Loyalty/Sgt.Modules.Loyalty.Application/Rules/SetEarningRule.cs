using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Rules;

public sealed record SetEarningRuleCommand(TenantId TenantId, decimal TryPerPoint, AuditContext Audit);

/// <summary>Puan oranını değiştirir; yeni oran sonraki rating'lerden itibaren geçerlidir (Spec 008 KK1).</summary>
public sealed class SetEarningRuleHandler(IEarningRuleStore rules, ILoyaltyUnitOfWork unitOfWork) : ICommandHandler<SetEarningRuleCommand>
{
    public async Task<Result> Handle(SetEarningRuleCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var rule = EarningRule.Set(command.TenantId, command.TryPerPoint);
        if (rule.IsFailure)
        {
            return rule;
        }

        rules.Save(rule.Value, command.Audit);
        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
