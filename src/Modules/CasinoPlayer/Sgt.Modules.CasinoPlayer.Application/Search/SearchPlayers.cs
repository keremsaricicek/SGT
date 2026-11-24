using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Ports;

namespace Sgt.Modules.CasinoPlayer.Application.Search;

public sealed record SearchPlayersQuery(TenantId TenantId, string Term);

/// <summary>Arama sonucu özet bilgi; belge numarası içermez (KK5).</summary>
public sealed record PlayerSummary(Guid PlayerId, string PlayerNumber, string FirstName, string LastName, string? ActiveCardNumber, bool IsRestricted);

public sealed class SearchPlayersHandler(IPlayerRepository players) : IQueryHandler<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>
{
    public const int MaxResults = 50;
    private const int MinTermLength = 2;

    public async Task<IReadOnlyList<PlayerSummary>> Handle(SearchPlayersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var term = query.Term?.Trim() ?? string.Empty;
        return term.Length < MinTermLength ? [] : await players.Search(query.TenantId, term, MaxResults, ct);
    }
}
