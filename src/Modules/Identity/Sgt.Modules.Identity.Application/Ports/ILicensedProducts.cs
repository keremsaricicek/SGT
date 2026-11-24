using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Application.Ports;

/// <summary>Müşterinin lisansında açık olan ürünler.</summary>
public interface ILicensedProducts
{
    Task<IReadOnlySet<Product>> For(TenantId tenantId, CancellationToken ct);
}
