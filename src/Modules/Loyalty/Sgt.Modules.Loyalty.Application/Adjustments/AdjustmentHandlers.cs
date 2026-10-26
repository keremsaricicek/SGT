using Sgt.BuildingBlocks;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Adjustments;

public sealed record RequestPointsAdjustmentCommand(TenantId TenantId, Guid PlayerId, long Points, string Note, AuditContext Audit);

/// <summary>Elle puan düzeltme isteği (Spec 008 KK5).</summary>
public sealed class RequestPointsAdjustmentHandler(IApprovalRequestRepository requests, ILoyaltyUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RequestPointsAdjustmentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestPointsAdjustmentCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var request = PointsAdjustmentRequest.Create(command.TenantId, command.PlayerId, command.Points, command.Note, command.Audit, clock.UtcNow);
        if (request.IsFailure)
        {
            return request.Error;
        }

        requests.Add(request.Value);
        await unitOfWork.Commit(ct);
        return request.Value.Id;
    }
}

public sealed record DecidePointsAdjustmentCommand(TenantId TenantId, Guid RequestId, bool Approve, AuditContext Audit);

/// <summary>Puan düzeltme onayı/reddi; isteyen kişi karar veremez (Spec 008 KK5).</summary>
public sealed class DecidePointsAdjustmentHandler(
    IApprovalRequestRepository requests, ILoyaltyAccountRepository accounts, ILoyaltyUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DecidePointsAdjustmentCommand>
{
    public async Task<Result> Handle(DecidePointsAdjustmentCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var request = await requests.GetAdjustment(command.TenantId, command.RequestId, ct);
        if (request is null)
        {
            return LoyaltyErrors.RequestNotFound;
        }

        var account = await accounts.Get(command.TenantId, request.PlayerId, ct);
        if (account is null)
        {
            account = LoyaltyAccount.Open(command.TenantId, request.PlayerId);
            accounts.Add(account);
        }

        var result = request.Decide(account, command.Approve, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
