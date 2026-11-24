using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;
using Sgt.Modules.CasinoPlayer.Domain;

namespace Sgt.Modules.CasinoPlayer.Application.Profile;

public sealed record GetPlayerQuery(TenantId TenantId, PlayerId PlayerId);

public sealed record RestrictionView(Guid Id, string Type, string? Note, DateTimeOffset? Until, Guid AddedBy, DateTimeOffset AddedAt);

/// <summary>Oyuncu kartı ekranı. Belge numarası maskelidir (KK4).</summary>
public sealed record PlayerView(
    Guid PlayerId,
    string PlayerNumber,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Nationality,
    string DocumentType,
    string MaskedDocumentNumber,
    string? Phone,
    string? ActiveCardNumber,
    bool IsRestricted,
    IReadOnlyList<RestrictionView> ActiveRestrictions);

public sealed class GetPlayerHandler(IPlayerRepository players, IClock clock) : IQueryHandler<GetPlayerQuery, PlayerView?>
{
    public async Task<PlayerView?> Handle(GetPlayerQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var player = await players.Get(query.TenantId, query.PlayerId, ct);
        if (player is null)
        {
            return null;
        }

        var now = clock.UtcNow;
        var d = player.Details;
        return new PlayerView(
            player.Id.Value,
            player.Number.Value,
            d.FirstName,
            d.LastName,
            d.BirthDate,
            d.Nationality,
            d.DocumentType.ToString(),
            d.MaskedDocumentNumber,
            d.Phone,
            player.ActiveCard?.Number.Value,
            player.IsRestricted(now),
            player.Restrictions
                .Where(r => r.IsActiveAt(now))
                .Select(r => new RestrictionView(r.Id, r.Type.ToString(), r.Note, r.Until, r.AddedBy.Value, r.AddedAt))
                .ToList());
    }
}
