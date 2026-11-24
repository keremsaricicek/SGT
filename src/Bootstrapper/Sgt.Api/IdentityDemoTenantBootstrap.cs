using Sgt.BuildingBlocks;
using Sgt.DemoData;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure;

namespace Sgt.Api;

/// <summary>
/// Demo tenant'ının S Plus lisansı ve ilk süper admini (Spec 012). Kimlik modülünün geliştirme tohumunu kullanır;
/// bu tohum denetim kaydı yazmaz (bilinen geliştirme istisnası, spec 012 notu).
/// </summary>
public sealed class IdentityDemoTenantBootstrap(IServiceProvider services) : IDemoTenantBootstrap
{
    public Task Prepare(TenantId tenantId, UserId superAdmin, CancellationToken ct) =>
        IdentityDevelopmentSeed.Run(services, tenantId, superAdmin, [Product.SPlus], ct);
}
