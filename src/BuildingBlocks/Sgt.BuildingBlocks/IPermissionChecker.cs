namespace Sgt.BuildingBlocks;

/// <summary>
/// Sunucu tarafı ekran yetkisi kontrolü (Spec 001 KK10). Uygulaması Identity modülündedir;
/// tüm modüller yetkiyi bu arayüzle sorar. Yetki kodu biçimi: "&lt;ekran&gt;:&lt;eylem&gt;".
/// </summary>
public interface IPermissionChecker
{
    Task<bool> HasPermission(TenantId tenantId, UserId userId, string permission, CancellationToken ct);
}
