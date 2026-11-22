using Sgt.BuildingBlocks;

namespace Sgt.DemoData;

/// <summary>
/// Demo tenant'ının ön koşulu (lisans + ilk süper admin). Kimlik modülünün geliştirme tohumunu çağırır; uygulaması
/// Bootstrapper'dadır (Sgt.DemoData modül Infrastructure'ına bağlanamaz, K2). Yalnızca veri yokken çağrılır.
/// </summary>
public interface IDemoTenantBootstrap
{
    Task Prepare(TenantId tenantId, UserId superAdmin, CancellationToken ct);
}
