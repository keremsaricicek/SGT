using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Cards;

public sealed record CancelPlayerCardCommand(TenantId TenantId, PlayerId PlayerId, AuditContext Audit);

public sealed class CancelPlayerCardHandler(IPlayerRepository players, IPlayerUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CancelPlayerCardCommand>
{
    public async Task<Result> Handle(CancelPlayerCardCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var player = await players.Get(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return PlayerErrors.PlayerNotFound;
        }

        var result = player.CancelCard(command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
