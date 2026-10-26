using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Profile;

public sealed record RegisterPlayerCommand(TenantId TenantId, PlayerDetails Details, AuditContext Audit);

public sealed record PlayerRegistered(Guid PlayerId, string PlayerNumber);

/// <summary>Oyuncu kaydı (Spec 005 KK1–KK3).</summary>
public sealed class RegisterPlayerHandler(IPlayerRepository players, IPlayerUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<RegisterPlayerCommand, PlayerRegistered>
{
    public async Task<Result<PlayerRegistered>> Handle(RegisterPlayerCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var normalized = command.Details.Normalize(clock.LocalToday());
        if (normalized.IsFailure)
        {
            return normalized.Error;
        }

        if (await players.DocumentInUse(command.TenantId, normalized.Value.DocumentType, normalized.Value.DocumentNumber, null, ct))
        {
            return PlayerErrors.DocumentInUse;
        }

        var number = PlayerNumber.FromSequence(await players.NextSequence(command.TenantId, ct));
        var player = Player.Register(PlayerId.New(), command.TenantId, number, normalized.Value, clock.LocalToday(), command.Audit, clock.UtcNow);
        if (player.IsFailure)
        {
            return player.Error;
        }

        players.Add(player.Value);
        await unitOfWork.Commit(ct);
        return new PlayerRegistered(player.Value.Id.Value, number.Value);
    }
}
