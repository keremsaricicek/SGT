namespace Sgt.BuildingBlocks.Web;

/// <summary>Kimlik sağlayıcının (OIDC) token'da vermesi gereken alanlar. Bkz. ADR-0003.</summary>
public static class SgtClaims
{
    public const string UserId = "sub";
    public const string TenantId = "tenant_id";
    public const string Policy = "SgtUser";
}

/// <summary>İstemcilerin her istekte gönderdiği başlıklar.</summary>
public static class SgtHeaders
{
    public const string Terminal = "X-Terminal";
    public const string CorrelationId = "X-Correlation-Id";
}
