using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

/// <summary>Vardiya açar; terminal, isteğin terminalidir (KK1).</summary>
public sealed record OpenShiftCommand(TenantId TenantId, IReadOnlyList<DrawerAmount> Opening, AuditContext Audit);

public sealed class OpenShiftHandler(IShiftRepository shifts, IFinanceUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<OpenShiftCommand, Guid>
{
    public async Task<Result<Guid>> Handle(OpenShiftCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (await shifts.HasOpenShift(command.TenantId, command.Audit.Actor, command.Audit.Terminal, ct))
        {
            return FinanceErrors.ShiftAlreadyOpen;
        }

        var shift = CashierShift.Open(Guid.NewGuid(), command.TenantId, command.Audit.Terminal, command.Opening, command.Audit, clock.UtcNow);
        if (shift.IsFailure)
        {
            return shift.Error;
        }

        shifts.Add(shift.Value);
        await unitOfWork.Commit(ct);
        return shift.Value.Id;
    }
}
