using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Cards;

public sealed record IssuePlayerCardCommand(TenantId TenantId, PlayerId PlayerId, string CardNumber, AuditContext Audit);

/// <summary>Kart verir; kart numarası tenant içinde benzersizdir, eski aktif kart iptal olur (KK7).</summary>
public sealed class IssuePlayerCardHandler(IPlayerRepository players, IPlayerUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<IssuePlayerCardCommand>
{
    public async Task<Result> Handle(IssuePlayerCardCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var number = CardNumber.Parse(command.CardNumber);
        if (number.IsFailure)
        {
            return number;
        }

        var player = await players.Get(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return PlayerErrors.PlayerNotFound;
        }

        if (await players.CardInUse(command.TenantId, number.Value, ct))
        {
            return PlayerErrors.CardInUse;
        }

        player.IssueCard(number.Value, command.Audit, clock.UtcNow);
        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
