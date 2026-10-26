using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

public static class IdentityErrors
{
    public static readonly Error InvalidPermissionCode =
        new("Identity.InvalidPermissionCode", "Yetki kodu '<ekran>:<eylem>' biçiminde olmalıdır.");

    public static readonly Error ViewRequired =
        new("Identity.ViewRequired", "Bu yetki için önce aynı ekranın görme yetkisi verilmelidir.");

    public static readonly Error ViewInUse =
        new("Identity.ViewInUse", "Ekranın diğer yetkileri dururken görme yetkisi geri alınamaz.");

    public static readonly Error NotSuperAdmin =
        new("Identity.NotSuperAdmin", "Rol ve yetkiyi yalnızca süper admin değiştirebilir.", ErrorKind.Forbidden);

    public static readonly Error TenantMismatch =
        new("Identity.TenantMismatch", "Farklı müşteriye (tenant) ait kayıtlar birlikte kullanılamaz.", ErrorKind.Forbidden);

    public static readonly Error RoleNameRequired =
        new("Identity.RoleNameRequired", "Rol adı boş olamaz.");

    public static readonly Error RoleNotFound =
        new("Identity.RoleNotFound", "Rol bulunamadı.", ErrorKind.NotFound);

    public static readonly Error UserNotFound =
        new("Identity.UserNotFound", "Kullanıcı bulunamadı.", ErrorKind.NotFound);

    public static readonly Error UnknownPermission =
        new("Identity.UnknownPermission", "Bu yetki ekran kataloğunda tanımlı değil.");
}
