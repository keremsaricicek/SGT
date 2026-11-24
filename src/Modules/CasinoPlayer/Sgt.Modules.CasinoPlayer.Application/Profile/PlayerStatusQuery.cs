using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Contracts;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Profile;

public sealed class PlayerStatusQuery(IPlayerRepository players, IClock clock) : IPlayerStatusQuery
{
    public async Task<PlayerStatusDto?> GetStatus(TenantId tenantId, Guid playerId, CancellationToken ct)
    {
        var player = await players.Get(tenantId, new PlayerId(playerId), ct);
        return player is null ? null : new PlayerStatusDto(player.Id.Value, player.Number.Value, player.IsRestricted(clock.UtcNow));
    }
}
