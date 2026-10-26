using Microsoft.EntityFrameworkCore;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure.Persistence;

namespace Sgt.Modules.Identity.Infrastructure.Licensing;

/// <summary>Müşterinin lisanslı ürünleri. Core her zaman açıktır ve tabloda tutulmaz.</summary>
internal sealed class LicensedProducts(IdentityDbContext db) : ILicensedProducts
{
    public async Task<IReadOnlySet<Product>> For(TenantId tenantId, CancellationToken ct)
    {
        var names = await db.TenantLicenses.AsNoTracking()
            .Where(l => l.TenantId == tenantId.Value)
            .Select(l => l.Product)
            .ToListAsync(ct);

        var products = names.Select(n => Enum.TryParse<Product>(n, out var p) ? p : (Product?)null)
            .OfType<Product>()
            .ToHashSet();
        products.Add(Product.Core);
        return products;
    }
}
