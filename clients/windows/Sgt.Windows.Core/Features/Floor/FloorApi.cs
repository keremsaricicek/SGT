using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Features.Floor;

/// <summary>Masa (GET /api/v1/floor/tables).</summary>
[ApiSchema("TableView")]
public sealed record TableDto(
    Guid TableId, string Code, string GameType, string Currency, decimal MinBet, decimal MaxBet, decimal HouseEdgePercent, int HandsPerHour, bool IsOpen);

[ApiSchema("FloorReasonRequest")]
public sealed record FloorReasonRequest(string ReasonCode);

[ApiSchema("StartRatingRequest")]
public sealed record StartRatingRequest(Guid TableId, Guid PlayerId, string ReasonCode);

/// <summary>Rating bitirme. PlayerResult: oyuncunun kazancı (+) / kaybı (−); bilinmiyorsa boş.</summary>
[ApiSchema("RatingValuesRequest")]
public sealed record RatingValuesRequest(decimal AverageBet, decimal? PlayerResult, string ReasonCode);

/// <summary>Masa listesi ve masa oturumu (aç/kapat).</summary>
public interface ITablesApi
{
    Task<ApiResult<IReadOnlyList<TableDto>>> ListTablesAsync(CancellationToken ct);

    Task<ApiResult<Done>> OpenTableAsync(Guid tableId, FloorReasonRequest request, CancellationToken ct);

    Task<ApiResult<Done>> CloseTableAsync(Guid tableId, FloorReasonRequest request, CancellationToken ct);
}

/// <summary>Oyuncu derecelendirme (rating) başlat/bitir.</summary>
public interface IRatingsApi
{
    Task<ApiResult<CreatedDto>> StartRatingAsync(StartRatingRequest request, CancellationToken ct);

    Task<ApiResult<Done>> FinishRatingAsync(Guid ratingId, RatingValuesRequest request, CancellationToken ct);
}

internal sealed class FloorApiClient(IApiTransport transport) : ITablesApi, IRatingsApi
{
    public Task<ApiResult<IReadOnlyList<TableDto>>> ListTablesAsync(CancellationToken ct) =>
        transport.GetAsync<IReadOnlyList<TableDto>>(ApiRoutes.ListTables.Build(), ct);

    public Task<ApiResult<Done>> OpenTableAsync(Guid tableId, FloorReasonRequest request, CancellationToken ct) =>
        transport.PostAsync(ApiRoutes.OpenTable.Build(tableId), request, ct);

    public Task<ApiResult<Done>> CloseTableAsync(Guid tableId, FloorReasonRequest request, CancellationToken ct) =>
        transport.PostAsync(ApiRoutes.CloseTable.Build(tableId), request, ct);

    public Task<ApiResult<CreatedDto>> StartRatingAsync(StartRatingRequest request, CancellationToken ct) =>
        transport.PostAsync<CreatedDto>(ApiRoutes.StartRating.Build(), request, ct);

    public Task<ApiResult<Done>> FinishRatingAsync(Guid ratingId, RatingValuesRequest request, CancellationToken ct) =>
        transport.PostAsync(ApiRoutes.FinishRating.Build(ratingId), request, ct);
}
