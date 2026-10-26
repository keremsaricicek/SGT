namespace Sgt.Modules.Identity.Infrastructure.Licensing;

/// <summary>Bir müşterinin lisansındaki tek ürün. Lisansları SGT yönetir; müşteri süper admini değiştiremez.</summary>
internal sealed class TenantLicenseRow
{
    public Guid TenantId { get; set; }

    public string Product { get; set; } = string.Empty;
}
