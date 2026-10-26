using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure.Licensing;
using Sgt.Modules.Identity.Infrastructure.Persistence;

namespace Sgt.Modules.Identity.Infrastructure;

/// <summary>
/// Geliştirme ve test ortamı için başlangıç verisi: müşteri lisansı ve ilk süper admin.
/// Üretimde müşteri kurulumu ayrı bir SGT aracıyla yapılır.
/// </summary>
public static class IdentityDevelopmentSeed
{
    public static async Task Run(IServiceProvider services, TenantId tenantId, UserId superAdmin, IEnumerable<Product> products, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        var db = services.GetRequiredService<IdentityDbContext>();

        foreach (var product in products.Where(p => p != Product.Core).Select(p => p.ToString()))
        {
            if (!await db.TenantLicenses.AnyAsync(l => l.TenantId == tenantId.Value && l.Product == product, ct))
            {
                db.TenantLicenses.Add(new TenantLicenseRow { TenantId = tenantId.Value, Product = product });
            }
        }

        if (!await db.Users.AnyAsync(u => u.TenantId == tenantId.Value && u.UserId == superAdmin.Value, ct))
        {
            var row = new UserAccessRow();
            row.CopyFrom(UserAccess.Create(superAdmin, tenantId, isSuperAdmin: true));
            db.Users.Add(row);
        }

        await db.SaveChangesAsync(ct);
    }
}
