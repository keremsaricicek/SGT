using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

public sealed record CloseShiftCommand(TenantId TenantId, Guid ShiftId, IReadOnlyList<DrawerAmount> Counted, string? Note, AuditContext Audit);

/// <summary>Vardiyayı sayımla kapatır (Spec 006 KK9).</summary>
public sealed class CloseShiftHandler(IShiftRepository shifts, IFinanceUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CloseShiftCommand>
{
    public async Task<Result> Handle(CloseShiftCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var shift = await shifts.Get(command.TenantId, command.ShiftId, ct);
        if (shift is null)
        {
            return FinanceErrors.ShiftNotFound;
        }

        var result = shift.Close(command.Counted, command.Note, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
