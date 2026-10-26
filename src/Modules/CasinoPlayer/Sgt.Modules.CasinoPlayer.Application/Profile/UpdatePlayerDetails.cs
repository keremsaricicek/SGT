using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Profile;

public sealed record UpdatePlayerDetailsCommand(TenantId TenantId, PlayerId PlayerId, PlayerDetails Details, AuditContext Audit);

/// <summary>Oyuncu bilgisi düzeltme (Spec 005 KK6); belge başka oyuncuda olamaz.</summary>
public sealed class UpdatePlayerDetailsHandler(IPlayerRepository players, IPlayerUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<UpdatePlayerDetailsCommand>
{
    public async Task<Result> Handle(UpdatePlayerDetailsCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var player = await players.Get(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return PlayerErrors.PlayerNotFound;
        }

        var normalized = command.Details.Normalize(clock.LocalToday());
        if (normalized.IsFailure)
        {
            return normalized.Error;
        }

        if (await players.DocumentInUse(command.TenantId, normalized.Value.DocumentType, normalized.Value.DocumentNumber, player.Id, ct))
        {
            return PlayerErrors.DocumentInUse;
        }

        var result = player.UpdateDetails(normalized.Value, clock.LocalToday(), command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
