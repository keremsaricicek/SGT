using Microsoft.AspNetCore.Http;

namespace Sgt.BuildingBlocks.Web;

/// <summary>
/// İstek bilgilerini token ve başlıklardan okur. Uç noktalar <see cref="SgtClaims.Policy"/> ile korunduğu için
/// kullanıcı ve tenant alanlarının varlığı garanti edilir.
/// </summary>
public sealed class HttpRequestContext(IHttpContextAccessor accessor) : IRequestContext
{
    private HttpContext Http => accessor.HttpContext ?? throw new InvalidOperationException("HTTP isteği dışında kullanılamaz.");

    public TenantId TenantId => new(ReadGuidClaim(SgtClaims.TenantId));

    public UserId UserId => new(ReadGuidClaim(SgtClaims.UserId));

    public AuditContext Audit(ReasonCode reason) => new(UserId, Terminal(), reason, CorrelationId());

    private Guid ReadGuidClaim(string type) =>
        Guid.TryParse(Http.User.FindFirst(type)?.Value, out var value)
            ? value
            : throw new InvalidOperationException($"Token'da geçerli '{type}' alanı yok.");

    private string Terminal()
    {
        var terminal = Http.Request.Headers[SgtHeaders.Terminal].ToString();
        return string.IsNullOrWhiteSpace(terminal)
            ? $"IP:{Http.Connection.RemoteIpAddress}"
            : terminal.Trim()[..Math.Min(terminal.Trim().Length, 100)];
    }

    private Guid CorrelationId()
    {
        if (Http.Items.TryGetValue(SgtHeaders.CorrelationId, out var existing) && existing is Guid known)
        {
            return known;
        }

        var id = Guid.TryParse(Http.Request.Headers[SgtHeaders.CorrelationId], out var sent) ? sent : Guid.NewGuid();
        Http.Items[SgtHeaders.CorrelationId] = id;
        return id;
    }
}
