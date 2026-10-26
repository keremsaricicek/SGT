using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Comps;

public sealed record RequestCompCommand(TenantId TenantId, Guid PlayerId, long Points, string Description, AuditContext Audit);

/// <summary>Comp isteği (Spec 008 KK6). Kısıtlı oyuncu isteyemez.</summary>
public sealed class RequestCompHandler(IApprovalRequestRepository requests, IPlayerStatusQuery players, ILoyaltyUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RequestCompCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestCompCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var player = await players.GetStatus(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return LoyaltyErrors.PlayerNotFound;
        }

        if (player.IsRestricted)
        {
            return LoyaltyErrors.PlayerRestricted;
        }

        var request = CompRequest.Create(command.TenantId, command.PlayerId, command.Points, command.Description, command.Audit, clock.UtcNow);
        if (request.IsFailure)
        {
            return request.Error;
        }

        requests.Add(request.Value);
        await unitOfWork.Commit(ct);
        return request.Value.Id;
    }
}

public sealed record DecideCompCommand(TenantId TenantId, Guid RequestId, bool Approve, AuditContext Audit);

/// <summary>Comp onayı/reddi; isteyen kişi karar veremez, onayda puan düşülür (Spec 008 KK6).</summary>
public sealed class DecideCompHandler(IApprovalRequestRepository requests, ILoyaltyAccountRepository accounts, ILoyaltyUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DecideCompCommand>
{
    public async Task<Result> Handle(DecideCompCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var request = await requests.GetComp(command.TenantId, command.RequestId, ct);
        if (request is null)
        {
            return LoyaltyErrors.RequestNotFound;
        }

        var account = await accounts.Get(command.TenantId, request.PlayerId, ct);
        var result = request.Decide(account, command.Approve, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
