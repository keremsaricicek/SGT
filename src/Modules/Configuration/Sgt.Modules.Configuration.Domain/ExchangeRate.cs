using Sgt.BuildingBlocks;

namespace Sgt.Modules.Configuration.Domain;

/// <summary>
/// Bir yabancı para biriminin ana birim (TRY) karşılığı alış/satış kuru. Değiştirilmez; yeni kur yeni kayıttır (KK3).
/// </summary>
public sealed class ExchangeRate
{
    private const int MaxDecimals = 6;

    private ExchangeRate(Guid id, TenantId tenantId, string currency, decimal buy, decimal sell, DateTimeOffset effectiveFrom, AuditContext audit)
    {
        Id = id;
        TenantId = tenantId;
        Currency = currency;
        Buy = buy;
        Sell = sell;
        EffectiveFrom = effectiveFrom;
        Audit = audit;
    }

    public Guid Id { get; }

    public TenantId TenantId { get; }

    public string Currency { get; }

    /// <summary>İşletmenin bu birimi alırken verdiği TRY.</summary>
    public decimal Buy { get; }

    /// <summary>İşletmenin bu birimi satarken aldığı TRY.</summary>
    public decimal Sell { get; }

    public DateTimeOffset EffectiveFrom { get; }

    public AuditContext Audit { get; }

    public static Result<ExchangeRate> Set(TenantId tenantId, string currency, decimal buy, decimal sell, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (!SupportedCurrencies.IsSupported(currency))
        {
            return ConfigurationErrors.UnsupportedCurrency;
        }

        var code = currency.Trim().ToUpperInvariant();
        if (code == SupportedCurrencies.Base)
        {
            return ConfigurationErrors.BaseCurrencyHasNoRate;
        }

        if (buy <= 0 || sell <= 0)
        {
            return ConfigurationErrors.RateMustBePositive;
        }

        if (buy > sell)
        {
            return ConfigurationErrors.BuyAboveSell;
        }

        if (decimal.Round(buy, MaxDecimals) != buy || decimal.Round(sell, MaxDecimals) != sell)
        {
            return ConfigurationErrors.TooManyDecimals;
        }

        return new ExchangeRate(Guid.NewGuid(), tenantId, code, buy, sell, at, audit);
    }

    /// <summary>Kalıcı kayıttan geri yükler. Yalnızca altyapı kullanır.</summary>
    public static ExchangeRate Restore(
        Guid id, TenantId tenantId, string currency, decimal buy, decimal sell, DateTimeOffset effectiveFrom, AuditContext audit) =>
        new(id, tenantId, currency, buy, sell, effectiveFrom, audit);
}
