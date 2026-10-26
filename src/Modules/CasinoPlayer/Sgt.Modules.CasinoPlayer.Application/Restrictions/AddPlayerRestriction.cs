using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Restrictions;

public sealed record AddPlayerRestrictionCommand(
    TenantId TenantId, PlayerId PlayerId, RestrictionType Type, string? Note, DateTimeOffset? Until, AuditContext Audit);

public sealed class AddPlayerRestrictionHandler(IPlayerRepository players, IPlayerUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<AddPlayerRestrictionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddPlayerRestrictionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var player = await players.Get(command.TenantId, command.PlayerId, ct);
        if (player is null)
        {
            return PlayerErrors.PlayerNotFound;
        }

        var result = player.AddRestriction(command.Type, command.Note, command.Until, command.Audit, clock.UtcNow);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.Commit(ct);
        return result.Value;
    }
}
