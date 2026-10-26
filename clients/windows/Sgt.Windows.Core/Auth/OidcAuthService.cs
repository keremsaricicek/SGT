using System.Security.Claims;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Options;
using Sgt.Windows.Core.Configuration;
using Sgt.Windows.Core.Localization;

namespace Sgt.Windows.Core.Auth;

/// <summary>
/// Keycloak ile OIDC Authorization Code + PKCE (Duende.IdentityModel.OidcClient).
/// Token'lar yalnızca bellekte tutulur; diske, loga veya ayar dosyasına yazılmaz.
/// Kimlik token'ı doğrudan token ucundan TLS ile alınır (code flow); asıl doğrulama API'dedir (JWT bearer).
/// </summary>
internal sealed class OidcAuthService(IOptions<AuthOptions> options, ISystemBrowser systemBrowser, TimeProvider clock)
    : IAuthService, ITokenProvider, IDisposable
{
    private static readonly TimeSpan RefreshBefore = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private OidcClient? _client;
    private TokenSet? _tokens;

    public SignedInUser? CurrentUser { get; private set; }

    public async Task<LoginOutcome> LoginAsync(CancellationToken ct)
    {
        var settings = options.Value;
        if (settings.Authority is null)
        {
            return new LoginOutcome(null, Strings.Login_NotConfigured);
        }

        var port = settings.LoopbackPort > 0 ? settings.LoopbackPort : LoopbackBrowser.FreePort();
        var browser = new LoopbackBrowser(systemBrowser, port);
        var client = new OidcClient(new OidcClientOptions
        {
            Authority = settings.Authority.ToString().TrimEnd('/'),
            ClientId = settings.ClientId,
            Scope = settings.Scope,
            RedirectUri = browser.RedirectUri,
            PostLogoutRedirectUri = browser.RedirectUri,
            Browser = browser,
            Policy = new Policy { Discovery = new DiscoveryPolicy { RequireHttps = settings.RequireHttps } },
        });

        var result = await client.LoginAsync(new LoginRequest(), ct);
        if (result.IsError)
        {
            return new LoginOutcome(null, string.IsNullOrWhiteSpace(result.ErrorDescription) ? result.Error : result.ErrorDescription);
        }

        await _gate.WaitAsync(ct);
        try
        {
            _client = client;
            _tokens = new TokenSet(result.AccessToken, result.RefreshToken, result.AccessTokenExpiration, result.IdentityToken);
            CurrentUser = new SignedInUser(DisplayName(result.User));
        }
        finally
        {
            _gate.Release();
        }

        return new LoginOutcome(CurrentUser, null);
    }

    public async Task LogoutAsync(CancellationToken ct)
    {
        OidcClient? client;
        string? idToken;
        await _gate.WaitAsync(ct);
        try
        {
            client = _client;
            idToken = _tokens?.IdentityToken;
            _client = null;
            _tokens = null;
            CurrentUser = null;
        }
        finally
        {
            _gate.Release();
        }

        // Ortak terminalde bir sonraki kullanıcının şifresiz girmemesi için Keycloak oturumu da kapatılır.
        if (client is not null && idToken is not null)
        {
            await client.LogoutAsync(new LogoutRequest { IdTokenHint = idToken }, ct);
        }
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_tokens is null || _client is null)
            {
                return null;
            }

            if (_tokens.ExpiresAt - clock.GetUtcNow() > RefreshBefore || string.IsNullOrEmpty(_tokens.RefreshToken))
            {
                return _tokens.AccessToken;
            }

            var refreshed = await _client.RefreshTokenAsync(_tokens.RefreshToken, cancellationToken: ct);
            if (refreshed.IsError)
            {
                // Oturum sunucuda sona ermiş: istek token'sız gider, API 401 döner, ekran yeniden giriş ister.
                _tokens = null;
                return null;
            }

            _tokens = new TokenSet(
                refreshed.AccessToken, refreshed.RefreshToken ?? _tokens.RefreshToken, refreshed.AccessTokenExpiration, refreshed.IdentityToken ?? _tokens.IdentityToken);
            return _tokens.AccessToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();

    private static string DisplayName(ClaimsPrincipal? user) =>
        user?.FindFirst("name")?.Value
        ?? user?.FindFirst("preferred_username")?.Value
        ?? user?.Identity?.Name
        ?? "?";

    private sealed record TokenSet(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt, string? IdentityToken);
}
