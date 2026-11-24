using Sgt.BuildingBlocks;

namespace Sgt.Modules.Configuration.Domain;

/// <summary>
/// Tenant işareti: bir kurulum adımının (ör. demo verisinin tamamı) bittiğini kalıcı olarak kaydeder.
/// Bir tenant'ta aynı kod bir kez konur ve değiştirilmez/silinmez (append-only).
/// </summary>
public sealed class TenantMarker
{
    private const int MinLength = 2;
    private const int MaxLength = 50;

    private TenantMarker(TenantId tenantId, string code, DateTimeOffset setAt, AuditContext audit)
    {
        TenantId = tenantId;
        Code = code;
        SetAt = setAt;
        Audit = audit;
    }

    public TenantId TenantId { get; }

    /// <summary>Büyük harf, rakam ve alt çizgi (ör. DEMO_DATA_V1).</summary>
    public string Code { get; }

    public DateTimeOffset SetAt { get; }

    public AuditContext Audit { get; }

    public static Result<TenantMarker> Set(TenantId tenantId, string code, AuditContext audit, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(audit);
        var normalized = Normalize(code);
        return normalized is null ? ConfigurationErrors.InvalidMarkerCode : new TenantMarker(tenantId, normalized, at, audit);
    }

    /// <summary>Geçerli kodu büyük harfe çevirir; geçersizse null.</summary>
    public static string? Normalize(string? code)
    {
        var value = (code ?? string.Empty).Trim().ToUpperInvariant();
        return value.Length is >= MinLength and <= MaxLength && value.All(c => char.IsAsciiLetterUpper(c) || char.IsAsciiDigit(c) || c == '_')
            ? value
            : null;
    }
}
