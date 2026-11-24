using Sgt.BuildingBlocks;

namespace Sgt.Modules.Finance.Domain;

public static class FinanceErrors
{
    public static readonly Error InvalidAmount = new("Finance.InvalidAmount", "Tutar sıfırdan büyük ve en çok 2 ondalıklı olmalıdır.");
    public static readonly Error InvalidOpeningAmount = new("Finance.InvalidOpeningAmount", "Açılış tutarı negatif olamaz ve en çok 2 ondalıklı olmalıdır.");
    public static readonly Error UnsupportedCurrency = new("Finance.UnsupportedCurrency", "Desteklenen para birimleri: TRY, EUR, GBP, USD.");
    public static readonly Error ForeignCurrencyRequired = new("Finance.ForeignCurrencyRequired", "Döviz işleminde yabancı para birimi gerekir.");
    public static readonly Error InsufficientDrawer = new("Finance.InsufficientDrawer", "Kasada bu işlem için yeterli nakit/çip yok.", ErrorKind.Conflict);
    public static readonly Error ShiftClosed = new("Finance.ShiftClosed", "Vardiya kapalı.", ErrorKind.Conflict);
    public static readonly Error NotShiftOwner = new("Finance.NotShiftOwner", "Vardiyayı yalnızca açan kasiyer işletir.", ErrorKind.Forbidden);
    public static readonly Error VarianceNoteRequired = new("Finance.VarianceNoteRequired", "Sayım farkı var; açıklama zorunludur.");
    public static readonly Error ShiftAlreadyOpen = new("Finance.ShiftAlreadyOpen", "Kasiyerin veya terminalin açık vardiyası var.", ErrorKind.Conflict);
    public static readonly Error ShiftNotFound = new("Finance.ShiftNotFound", "Vardiya bulunamadı.", ErrorKind.NotFound);
    public static readonly Error PlayerNotFound = new("Finance.PlayerNotFound", "Oyuncu bulunamadı.", ErrorKind.NotFound);
    public static readonly Error PlayerRestricted = new("Finance.PlayerRestricted", "Oyuncu kısıtlı; işlem yapılamaz.", ErrorKind.Forbidden);
    public static readonly Error RateUnavailable = new("Finance.RateUnavailable", "Bu para birimi için geçerli kur yok.", ErrorKind.Conflict);
    public static readonly Error DuplicateKeyMismatch = new("Finance.DuplicateKeyMismatch", "Bu işlem anahtarı farklı bir işlem için kullanılmış.", ErrorKind.Conflict);
    public static readonly Error TransactionNotFound = new("Finance.TransactionNotFound", "İşlem bu vardiyada bulunamadı.", ErrorKind.NotFound);
    public static readonly Error AlreadyReversed = new("Finance.AlreadyReversed", "İşlem zaten ters çevrilmiş.", ErrorKind.Conflict);
    public static readonly Error CannotReverseReversal = new("Finance.CannotReverseReversal", "Ters kayıt ters çevrilemez.", ErrorKind.Conflict);
    public static readonly Error AdjustmentNoteRequired = new("Finance.AdjustmentNoteRequired", "Düzeltme için açıklama zorunludur.");
    public static readonly Error AdjustmentNotFound = new("Finance.AdjustmentNotFound", "Düzeltme isteği bulunamadı.", ErrorKind.NotFound);
}
