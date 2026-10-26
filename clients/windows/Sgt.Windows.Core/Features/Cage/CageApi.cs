using Sgt.Windows.Core.Api;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>Çekmecedeki değer türü (sözleşmede "Instrument").</summary>
public enum Instrument
{
    Cash,
    Chips,
}

[ApiSchema("DrawerAmount")]
public sealed record DrawerAmount(Instrument Instrument, string Currency, decimal Amount);

[ApiSchema("OpenShiftRequest")]
public sealed record OpenShiftRequest(IReadOnlyList<DrawerAmount> Opening, string ReasonCode);

/// <summary>Kasa işlemi isteği. TransactionId: kullanıcı işlemi başına bir kez üretilen idempotency anahtarı.</summary>
[ApiSchema("CageRequest")]
public sealed record CageRequest(Guid TransactionId, Guid PlayerId, decimal Amount, string Currency, string ReasonCode);

[ApiSchema("CloseShiftRequest")]
public sealed record CloseShiftRequest(IReadOnlyList<DrawerAmount> Counted, string? Note, string ReasonCode);

/// <summary>Düzeltme talebi. TransactionId: düzeltilecek (ters kaydı yapılacak) işlemin kimliği.</summary>
[ApiSchema("AdjustmentRequest")]
public sealed record AdjustmentRequest(Guid TransactionId, string Note, string ReasonCode);

[ApiSchema("DrawerMovement")]
public sealed record DrawerMovementDto(string Instrument, string Currency, decimal Amount);

[ApiSchema("CageTransactionResult")]
public sealed record CageTransactionDto(
    Guid TransactionId, string Type, decimal Amount, string Currency, decimal? Rate, IReadOnlyList<DrawerMovementDto> Movements);

[ApiSchema("BalanceView")]
public sealed record BalanceDto(string Instrument, string Currency, decimal Expected);

[ApiSchema("ClosingCountView")]
public sealed record ClosingCountDto(string Instrument, string Currency, decimal Expected, decimal Counted, decimal Variance);

/// <summary>Vardiya özeti (GET /api/v1/cage/shifts/{id}).</summary>
[ApiSchema("ShiftSummary")]
public sealed record ShiftSummaryDto(
    Guid ShiftId,
    Guid CashierId,
    string Terminal,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    string? ClosingNote,
    IReadOnlyList<BalanceDto> Balances,
    IReadOnlyList<CageTransactionDto> Transactions,
    IReadOnlyList<ClosingCountDto> ClosingCounts);

/// <summary>Vardiya açma, özet ve kapanış uçları.</summary>
public interface ICageShiftApi
{
    Task<ApiResult<CreatedDto>> OpenShiftAsync(OpenShiftRequest request, CancellationToken ct);

    Task<ApiResult<ShiftSummaryDto>> GetShiftAsync(Guid shiftId, CancellationToken ct);

    Task<ApiResult<Done>> CloseShiftAsync(Guid shiftId, CloseShiftRequest request, CancellationToken ct);
}

/// <summary>Oyuncuyla yapılan kasa işlemleri (çip satış/bozdurma, döviz).</summary>
public interface ICageTransactionApi
{
    Task<ApiResult<CageTransactionDto>> SellChipsAsync(Guid shiftId, CageRequest request, CancellationToken ct);

    Task<ApiResult<CageTransactionDto>> RedeemChipsAsync(Guid shiftId, CageRequest request, CancellationToken ct);

    Task<ApiResult<CageTransactionDto>> ExchangeForeignToBaseAsync(Guid shiftId, CageRequest request, CancellationToken ct);

    Task<ApiResult<CageTransactionDto>> ExchangeBaseToForeignAsync(Guid shiftId, CageRequest request, CancellationToken ct);
}

/// <summary>Düzeltme (ters kayıt) talebi. Onay ikinci kişidedir (dört göz) ve web/yetkili ekrandan yapılır.</summary>
public interface ICageAdjustmentApi
{
    Task<ApiResult<CreatedDto>> RequestAdjustmentAsync(Guid shiftId, AdjustmentRequest request, CancellationToken ct);
}

internal sealed class CageApiClient(IApiTransport transport) : ICageShiftApi, ICageTransactionApi, ICageAdjustmentApi
{
    public Task<ApiResult<CreatedDto>> OpenShiftAsync(OpenShiftRequest request, CancellationToken ct) =>
        transport.PostAsync<CreatedDto>(ApiRoutes.OpenShift.Build(), request, ct);

    public Task<ApiResult<ShiftSummaryDto>> GetShiftAsync(Guid shiftId, CancellationToken ct) =>
        transport.GetAsync<ShiftSummaryDto>(ApiRoutes.GetShift.Build(shiftId), ct);

    public Task<ApiResult<Done>> CloseShiftAsync(Guid shiftId, CloseShiftRequest request, CancellationToken ct) =>
        transport.PostAsync(ApiRoutes.CloseShift.Build(shiftId), request, ct);

    public Task<ApiResult<CageTransactionDto>> SellChipsAsync(Guid shiftId, CageRequest request, CancellationToken ct) =>
        transport.PostAsync<CageTransactionDto>(ApiRoutes.ChipSales.Build(shiftId), request, ct);

    public Task<ApiResult<CageTransactionDto>> RedeemChipsAsync(Guid shiftId, CageRequest request, CancellationToken ct) =>
        transport.PostAsync<CageTransactionDto>(ApiRoutes.ChipRedemptions.Build(shiftId), request, ct);

    public Task<ApiResult<CageTransactionDto>> ExchangeForeignToBaseAsync(Guid shiftId, CageRequest request, CancellationToken ct) =>
        transport.PostAsync<CageTransactionDto>(ApiRoutes.ExchangeForeignToBase.Build(shiftId), request, ct);

    public Task<ApiResult<CageTransactionDto>> ExchangeBaseToForeignAsync(Guid shiftId, CageRequest request, CancellationToken ct) =>
        transport.PostAsync<CageTransactionDto>(ApiRoutes.ExchangeBaseToForeign.Build(shiftId), request, ct);

    public Task<ApiResult<CreatedDto>> RequestAdjustmentAsync(Guid shiftId, AdjustmentRequest request, CancellationToken ct) =>
        transport.PostAsync<CreatedDto>(ApiRoutes.RequestAdjustment.Build(shiftId), request, ct);
}
