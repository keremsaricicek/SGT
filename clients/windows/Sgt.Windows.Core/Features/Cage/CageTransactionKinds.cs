using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Features.Shell;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Features.Cage;

/// <summary>
/// Oyuncuyla yapılan bir kasa işlemi türü (strateji). Aynı ekran mantığı (oyuncu + tutar + para birimi + gerekçe)
/// tüm türler için ortaktır; tür yalnızca yetkiyi, başlığı ve çağrılan API ucunu belirler. Yeni tür = yeni sınıf (OCP).
/// </summary>
public interface ICageTransactionKind
{
    string ScreenCode { get; }

    string Action { get; }

    string Title { get; }

    string DefaultReasonCode { get; }

    Task<ApiResult<CageTransactionDto>> ExecuteAsync(ICageTransactionApi api, Guid shiftId, CageRequest request, CancellationToken ct);
}

public sealed class SellChips : ICageTransactionKind
{
    public string ScreenCode => ScreenCodes.CageChips;

    public string Action => "sell";

    public string Title => Strings.Cage_SellChips;

    public string DefaultReasonCode => "CHIP_SALE";

    public Task<ApiResult<CageTransactionDto>> ExecuteAsync(ICageTransactionApi api, Guid shiftId, CageRequest request, CancellationToken ct) =>
        api.SellChipsAsync(shiftId, request, ct);

    public override string ToString() => Title;
}

public sealed class RedeemChips : ICageTransactionKind
{
    public string ScreenCode => ScreenCodes.CageChips;

    public string Action => "redeem";

    public string Title => Strings.Cage_RedeemChips;

    public string DefaultReasonCode => "CHIP_REDEEM";

    public Task<ApiResult<CageTransactionDto>> ExecuteAsync(ICageTransactionApi api, Guid shiftId, CageRequest request, CancellationToken ct) =>
        api.RedeemChipsAsync(shiftId, request, ct);

    public override string ToString() => Title;
}

/// <summary>Döviz alış: oyuncunun dövizi alınır, ana para birimi verilir. Para birimi = alınan döviz.</summary>
public sealed class ExchangeForeignToBase : ICageTransactionKind
{
    public string ScreenCode => ScreenCodes.CageExchange;

    public string Action => "exchange";

    public string Title => Strings.Cage_ForeignToBase;

    public string DefaultReasonCode => "FX_BUY";

    public Task<ApiResult<CageTransactionDto>> ExecuteAsync(ICageTransactionApi api, Guid shiftId, CageRequest request, CancellationToken ct) =>
        api.ExchangeForeignToBaseAsync(shiftId, request, ct);

    public override string ToString() => Title;
}

/// <summary>Döviz satış: ana para birimi alınır, döviz verilir. Para birimi = verilen döviz.</summary>
public sealed class ExchangeBaseToForeign : ICageTransactionKind
{
    public string ScreenCode => ScreenCodes.CageExchange;

    public string Action => "exchange";

    public string Title => Strings.Cage_BaseToForeign;

    public string DefaultReasonCode => "FX_SELL";

    public Task<ApiResult<CageTransactionDto>> ExecuteAsync(ICageTransactionApi api, Guid shiftId, CageRequest request, CancellationToken ct) =>
        api.ExchangeBaseToForeignAsync(shiftId, request, ct);

    public override string ToString() => Title;
}
