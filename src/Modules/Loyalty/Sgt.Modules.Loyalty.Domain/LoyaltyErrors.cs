using Sgt.BuildingBlocks;

namespace Sgt.Modules.Loyalty.Domain;

public static class LoyaltyErrors
{
    public static readonly Error InsufficientPoints = new("Loyalty.InsufficientPoints", "Puan bakiyesi yetersiz.", ErrorKind.Conflict);
    public static readonly Error InvalidPoints = new("Loyalty.InvalidPoints", "Puan geçersiz.");
    public static readonly Error DescriptionRequired = new("Loyalty.DescriptionRequired", "Açıklama zorunludur.");
    public static readonly Error FourEyesRequired = new("Loyalty.FourEyesRequired", "İsteği yapan kişi onaylayamaz; başka bir yetkili gerekir.", ErrorKind.Forbidden);
    public static readonly Error AlreadyDecided = new("Loyalty.AlreadyDecided", "Bu istek zaten sonuçlandı.", ErrorKind.Conflict);
    public static readonly Error RequestNotFound = new("Loyalty.RequestNotFound", "İstek bulunamadı.", ErrorKind.NotFound);
    public static readonly Error PlayerNotFound = new("Loyalty.PlayerNotFound", "Oyuncu bulunamadı.", ErrorKind.NotFound);
    public static readonly Error PlayerRestricted = new("Loyalty.PlayerRestricted", "Kısıtlı oyuncu comp isteyemez.", ErrorKind.Forbidden);
    public static readonly Error InvalidRule = new("Loyalty.InvalidRule", "Puan oranı sıfırdan büyük ve en çok 2 ondalıklı olmalıdır.");
}
