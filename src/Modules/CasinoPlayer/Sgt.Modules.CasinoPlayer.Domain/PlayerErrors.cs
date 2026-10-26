using Sgt.BuildingBlocks;

namespace Sgt.Modules.CasinoPlayer.Domain;

public static class PlayerErrors
{
    public static readonly Error NameRequired = new("Player.NameRequired", "Ad ve soyad zorunludur (en çok 100 karakter).");
    public static readonly Error InvalidNationality = new("Player.InvalidNationality", "Uyruk 2 harfli ülke kodu olmalıdır (ör. CY, TR).");
    public static readonly Error DocumentRequired = new("Player.DocumentRequired", "Kimlik belgesi numarası zorunludur.");
    public static readonly Error Underage = new("Player.Underage", "18 yaşından küçük oyuncu kaydedilemez.");
    public static readonly Error DocumentInUse = new("Player.DocumentInUse", "Bu belge ile kayıtlı başka bir oyuncu var.", ErrorKind.Conflict);
    public static readonly Error InvalidCardNumber = new("Player.InvalidCardNumber", "Kart numarası 4-20 harf/rakam olmalıdır.");
    public static readonly Error CardInUse = new("Player.CardInUse", "Bu kart numarası kullanımda.", ErrorKind.Conflict);
    public static readonly Error NoActiveCard = new("Player.NoActiveCard", "Oyuncunun aktif kartı yok.");
    public static readonly Error RestrictionNotFound = new("Player.RestrictionNotFound", "Aktif kısıtlama bulunamadı.", ErrorKind.NotFound);
    public static readonly Error FourEyesRequired = new("Player.FourEyesRequired", "Kısıtlamayı ekleyen kişi kaldıramaz; başka bir yetkili gerekir.", ErrorKind.Forbidden);
    public static readonly Error RestrictionEndInPast = new("Player.RestrictionEndInPast", "Kısıtlama bitişi gelecekte olmalıdır.");
    public static readonly Error PlayerNotFound = new("Player.NotFound", "Oyuncu bulunamadı.", ErrorKind.NotFound);
}
