using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.Loyalty.Application.Ports;
using Sgt.Modules.Loyalty.Domain;

namespace Sgt.Modules.Loyalty.Application.Comps;

/// <summary>RequestId istemcinin idempotency anahtarıdır.</summary>
public sealed record RequestCompCommand(TenantId TenantId, Guid RequestId, Guid PlayerId, long Points, string Description, AuditContext Audit);

/// <summary>Comp isteği (Spec 008 KK6). Kısıtlı oyuncu isteyemez.</summary>
public sealed class RequestCompHandler(IApprovalRequestRepository requests, IPlayerStatusQuery players, ILoyaltyUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RequestCompCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RequestCompCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (await requests.GetComp(command.TenantId, command.RequestId, ct) is { } existing)
        {
            return existing.PlayerId == command.PlayerId && existing.Points == command.Points ? existing.Id : LoyaltyErrors.DuplicateRequestMismatch;
        }

        var player = await players.GetStatus(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return LoyaltyErrors.PlayerNotFound;
        }

        if (player.IsRestricted)
        {
            return LoyaltyErrors.PlayerRestricted;
        }

        var request = CompRequest.Create(command.RequestId, command.TenantId, command.PlayerId, command.Points, command.Description, command.Audit, clock.UtcNow);
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
public sealed class DecideCompHandler(
    IApprovalRequestRepository requests, ILoyaltyAccountRepository accounts, IPlayerStatusQuery players, ILoyaltyUnitOfWork unitOfWork, IClock clock)
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

        // İstek ile onay arasında oyuncu kısıtlanmış olabilir (KK6).
        if (command.Approve && (await players.GetStatus(command.TenantId, request.PlayerId, ct))?.IsRestricted != false)
        {
            return LoyaltyErrors.PlayerRestricted;
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
