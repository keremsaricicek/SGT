using Sgt.BuildingBlocks;
using Sgt.Modules.Finance.Application.Ports;
using Sgt.Modules.Finance.Domain;

namespace Sgt.Modules.Finance.Application.Cage;

public sealed record RequestCageAdjustmentCommand(TenantId TenantId, Guid ShiftId, Guid TransactionId, string Note, AuditContext Audit);

/// <summary>Kasa işlemi için düzeltme (ters kayıt) isteği (Spec 010 KK1).</summary>
public sealed class RequestCageAdjustmentHandler(
    IShiftRepository shifts, ICageAdjustmentRepository adjustments, IFinanceUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RequestCageAdjustmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestCageAdjustmentCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var shift = await shifts.Get(command.TenantId, command.ShiftId, ct);
        if (shift is null)
        {
            return FinanceErrors.ShiftNotFound;
        }

        var request = CageAdjustmentRequest.Create(shift, command.TransactionId, command.Note, command.Audit, clock.UtcNow);
        if (request.IsFailure)
        {
            return request.Error;
        }

        adjustments.Add(request.Value);
        await unitOfWork.Commit(ct);
        return request.Value.Id;
    }
}

public sealed record DecideCageAdjustmentCommand(TenantId TenantId, Guid RequestId, bool Approve, AuditContext Audit);

/// <summary>Düzeltme onayı/reddi; isteyen onaylayamaz, onayda ters kayıt yapılır (Spec 010 KK2–KK5).</summary>
public sealed class DecideCageAdjustmentHandler(
    IShiftRepository shifts, ICageAdjustmentRepository adjustments, IFinanceUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DecideCageAdjustmentCommand>
{
    public async Task<Result> Handle(DecideCageAdjustmentCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var request = await adjustments.Get(command.TenantId, command.RequestId, ct);
        if (request is null)
        {
            return FinanceErrors.AdjustmentNotFound;
        }

        var shift = await shifts.Get(command.TenantId, request.ShiftId, ct);
        if (shift is null)
        {
            return FinanceErrors.ShiftNotFound;
        }

        var result = request.Decide(shift, command.Approve, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
