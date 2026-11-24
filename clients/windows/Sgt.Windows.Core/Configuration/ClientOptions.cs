namespace Sgt.Windows.Core.Configuration;

/// <summary>API bağlantısı (appsettings.json → "Api"). İstemcide secret veya veritabanı bilgisi bulunmaz.</summary>
public sealed class ApiOptions
{
    public const string Section = "Api";

    /// <summary>API taban adresi, ör. https://sgt.ornek-casino.local/</summary>
    public Uri? BaseAddress { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}

/// <summary>Giriş (OIDC, Keycloak) ayarları (appsettings.json → "Authentication"). ADR-0003.</summary>
public sealed class AuthOptions
{
    public const string Section = "Authentication";

    /// <summary>Keycloak realm adresi, ör. https://giris.ornek-casino.local/realms/sgt</summary>
    public Uri? Authority { get; init; }

    /// <summary>Keycloak'taki public istemci (Authorization Code + PKCE).</summary>
    public string ClientId { get; init; } = "sgt-windows";

    public string Scope { get; init; } = "openid profile offline_access";

    /// <summary>Tarayıcıdan dönüş için yerel port (http://127.0.0.1:PORT/). 0: boş bir port seçilir.</summary>
    public int LoopbackPort { get; init; }

    /// <summary>Yalnızca geliştirme ortamında (http://localhost Keycloak) false yapılır.</summary>
    public bool RequireHttps { get; init; } = true;
}

/// <summary>Arayüz dili (appsettings.json → "Ui"). Varsayılan Türkçe.</summary>
public sealed class UiOptions
{
    public const string Section = "Ui";

    public string Language { get; init; } = "tr";
}
