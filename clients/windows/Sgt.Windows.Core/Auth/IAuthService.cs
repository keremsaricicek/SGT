namespace Sgt.Windows.Core.Auth;

/// <summary>Oturum açmış kullanıcı (yalnızca ekranda gösterilecek bilgiler).</summary>
public sealed record SignedInUser(string DisplayName);

/// <summary>Giriş sonucu. Başarısızsa <see cref="Error"/> kullanıcıya gösterilir.</summary>
public sealed record LoginOutcome(SignedInUser? User, string? Error)
{
    public bool Succeeded => User is not null;
}

/// <summary>
/// Giriş/çıkış. Şifre bu uygulamaya girilmez: sistem tarayıcısı Keycloak giriş sayfasını açar (OIDC + PKCE, ADR-0003).
/// MFA ve şifre politikası Keycloak'tadır.
/// </summary>
public interface IAuthService
{
    SignedInUser? CurrentUser { get; }

    Task<LoginOutcome> LoginAsync(CancellationToken ct);

    Task LogoutAsync(CancellationToken ct);
}

/// <summary>API isteklerine eklenecek erişim token'ı. Süresi dolmak üzereyse yenilenir; oturum yoksa null.</summary>
public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken ct);
}

/// <summary>Bir adresi kullanıcının varsayılan tarayıcısında açar.</summary>
public interface ISystemBrowser
{
    void Open(Uri url);
}
