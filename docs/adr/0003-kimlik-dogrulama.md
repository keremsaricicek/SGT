# ADR-0003: Kimlik doğrulama (giriş)

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Bağlam
Casinolar sunucuları kendi binalarında (on-prem) çalıştırmak isteyebilir; KKTC'de bulut zorunluluğu yoktur.
Windows, mobil ve web istemcileri aynı girişi kullanmalıdır. MFA gereklidir (anayasa VI).

## Karar
- Kimlik sağlayıcı: **Keycloak** (açık kaynak, on-prem veya bulutta çalışır, OIDC, MFA, şifre politikası hazır).
- API yalnızca JWT bearer token doğrular; şifre tutmaz, giriş ekranı yazmaz.
- Token'da zorunlu alanlar: `sub` (kullanıcı GUID) ve `tenant_id` (müşteri GUID). Keycloak'ta kullanıcı
  özniteliği + protocol mapper ile verilir. Bu alanlar yoksa istek reddedilir (`SgtUser` politikası).
- Yetki (hangi ekran, hangi işlem) Keycloak'ta değil, SGT yetki panelindedir (Spec 001). Keycloak yalnızca
  "kim olduğunu" söyler.
- İstemci her istekte `X-Terminal` (cihaz/terminal adı) ve isteğe bağlı `X-Correlation-Id` gönderir.
- Yapılandırma: `Authentication:Authority`, `Authentication:Audience` (appsettings / ortam değişkeni).

## Sonuçlar
- Kurulum paketine Keycloak dahil edilir (Docker veya Windows servisi); ayrı kurulum dokümanı yazılacak.
- Testler gerçek Keycloak yerine test kimlik doğrulama şeması kullanır.
