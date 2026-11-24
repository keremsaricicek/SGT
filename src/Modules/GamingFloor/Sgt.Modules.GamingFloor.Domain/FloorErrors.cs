using Sgt.BuildingBlocks;

namespace Sgt.Modules.GamingFloor.Domain;

public static class FloorErrors
{
    public static readonly Error InvalidTableCode = new("Floor.InvalidTableCode", "Masa kodu 2-20 karakter (harf, rakam, -) olmalıdır.");
    public static readonly Error InvalidLimits = new("Floor.InvalidLimits", "Min bahis sıfırdan büyük ve maks bahisten küçük/eşit olmalıdır.");
    public static readonly Error InvalidHouseEdge = new("Floor.InvalidHouseEdge", "Kasa avantajı 0-100 arasında olmalıdır.");
    public static readonly Error InvalidHandsPerHour = new("Floor.InvalidHandsPerHour", "Saatlik el sayısı 1-1000 arasında olmalıdır.");
    public static readonly Error UnsupportedCurrency = new("Floor.UnsupportedCurrency", "Desteklenen para birimleri: TRY, EUR, GBP, USD.");
    public static readonly Error TableCodeTaken = new("Floor.TableCodeTaken", "Bu masa kodu kullanımda.", ErrorKind.Conflict);
    public static readonly Error TableNotFound = new("Floor.TableNotFound", "Masa bulunamadı.", ErrorKind.NotFound);
    public static readonly Error TableAlreadyOpen = new("Floor.TableAlreadyOpen", "Masa zaten açık.", ErrorKind.Conflict);
    public static readonly Error TableNotOpen = new("Floor.TableNotOpen", "Masa açık değil.", ErrorKind.Conflict);
    public static readonly Error RatingsStillRunning = new("Floor.RatingsStillRunning", "Masada süren rating var; önce bitirin.", ErrorKind.Conflict);
    public static readonly Error PlayerNotFound = new("Floor.PlayerNotFound", "Oyuncu bulunamadı.", ErrorKind.NotFound);
    public static readonly Error PlayerRestricted = new("Floor.PlayerRestricted", "Oyuncu kısıtlı.", ErrorKind.Forbidden);
    public static readonly Error PlayerAlreadyRated = new("Floor.PlayerAlreadyRated", "Oyuncunun süren bir rating'i var.", ErrorKind.Conflict);
    public static readonly Error RatingNotFound = new("Floor.RatingNotFound", "Rating bulunamadı.", ErrorKind.NotFound);
    public static readonly Error RatingNotRunning = new("Floor.RatingNotRunning", "Rating zaten bitmiş.", ErrorKind.Conflict);
    public static readonly Error RatingNotFinished = new("Floor.RatingNotFinished", "Yalnızca bitmiş rating düzeltilebilir.", ErrorKind.Conflict);
    public static readonly Error BetOutsideLimits = new("Floor.BetOutsideLimits", "Ortalama bahis masa limitleri dışında veya 2 haneden fazla ondalıklı.");
    public static readonly Error InvalidResult = new("Floor.InvalidResult", "Oyuncu sonucu en çok 2 ondalıklı olmalıdır.");
}
