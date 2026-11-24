using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Restrictions;

public sealed record RemovePlayerRestrictionCommand(TenantId TenantId, PlayerId PlayerId, Guid RestrictionId, AuditContext Audit);

/// <summary>Kısıtlamayı kaldırır; ekleyen kişi kaldıramaz (KK9).</summary>
public sealed class RemovePlayerRestrictionHandler(IPlayerRepository players, IPlayerUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RemovePlayerRestrictionCommand>
{
    public async Task<Result> Handle(RemovePlayerRestrictionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var player = await players.Get(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return PlayerErrors.PlayerNotFound;
        }

        var result = player.RemoveRestriction(command.RestrictionId, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
