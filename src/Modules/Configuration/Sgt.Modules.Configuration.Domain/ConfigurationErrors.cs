using Sgt.BuildingBlocks;

namespace Sgt.Modules.Configuration.Domain;

public static class ConfigurationErrors
{
    public static readonly Error UnsupportedCurrency =
        new("Configuration.UnsupportedCurrency", "Desteklenen para birimleri: TRY, EUR, GBP, USD.");

    public static readonly Error BaseCurrencyHasNoRate =
        new("Configuration.BaseCurrencyHasNoRate", "Ana para birimi (TRY) için kur girilmez.");

    public static readonly Error RateMustBePositive =
        new("Configuration.RateMustBePositive", "Kur sıfırdan büyük olmalıdır.");

    public static readonly Error BuyAboveSell =
        new("Configuration.BuyAboveSell", "Alış kuru satış kurundan büyük olamaz.");

    public static readonly Error TooManyDecimals =
        new("Configuration.TooManyDecimals", "Kur en çok 6 ondalık basamak içerebilir.");
}
