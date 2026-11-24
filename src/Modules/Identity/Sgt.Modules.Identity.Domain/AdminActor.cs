using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

/// <summary>
/// Rol ve yetki değiştirmeye yetkili kişi. Yalnızca süper admin için oluşturulabilir;
/// böylece "yalnızca süper admin değiştirir" kuralı tip düzeyinde garanti edilir.
/// </summary>
public sealed class AdminActor
{
    private AdminActor(TenantId tenantId, AuditContext audit)
    {
        TenantId = tenantId;
        Audit = audit;
    }

    public TenantId TenantId { get; }

    public AuditContext Audit { get; }

    public static Result<AdminActor> From(TenantId tenantId, AuditContext audit, bool isSuperAdmin)
    {
        ArgumentNullException.ThrowIfNull(audit);
        if (!isSuperAdmin)
        {
            return IdentityErrors.NotSuperAdmin;
        }

        return new AdminActor(tenantId, audit);
    }
}
