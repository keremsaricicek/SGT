using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Features.Players;

/// <summary>Arama sonucu (GET /api/v1/players?q=). Belge numarası içermez.</summary>
[ApiSchema("PlayerSummary")]
public sealed record PlayerSummaryDto(
    Guid PlayerId, string PlayerNumber, string FirstName, string LastName, string? ActiveCardNumber, bool IsRestricted)
{
    public string FullName => $"{FirstName} {LastName}";
}

[ApiSchema("RestrictionView")]
public sealed record RestrictionDto(Guid Id, string Type, string? Note, DateTimeOffset? Until, Guid AddedBy, DateTimeOffset AddedAt);

/// <summary>Oyuncu kartı (GET /api/v1/players/{id}). Belge numarası sunucuda maskelenmiştir.</summary>
[ApiSchema("PlayerView")]
public sealed record PlayerDetailDto(
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
    IReadOnlyList<RestrictionDto> ActiveRestrictions);

public interface IPlayersApi
{
    Task<ApiResult<IReadOnlyList<PlayerSummaryDto>>> SearchAsync(string term, CancellationToken ct);

    Task<ApiResult<PlayerDetailDto>> GetAsync(Guid playerId, CancellationToken ct);
}

internal sealed class PlayersApiClient(IApiTransport transport) : IPlayersApi
{
    public Task<ApiResult<IReadOnlyList<PlayerSummaryDto>>> SearchAsync(string term, CancellationToken ct)
    {
        var path = new Uri($"{ApiRoutes.SearchPlayers.Build()}?q={Uri.EscapeDataString(term)}", UriKind.Relative);
        return transport.GetAsync<IReadOnlyList<PlayerSummaryDto>>(path, ct);
    }

    public Task<ApiResult<PlayerDetailDto>> GetAsync(Guid playerId, CancellationToken ct) =>
        transport.GetAsync<PlayerDetailDto>(ApiRoutes.GetPlayer.Build(playerId), ct);
}
