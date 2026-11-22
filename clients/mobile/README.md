# S Plus mobil uygulaması (Flutter, iOS/Android)

Spec: `specs/014-mobil/spec.md`. Veritabanına bağlanmaz; yalnızca `/api/v1` kullanır, yetki Windows/web ile aynıdır.

## Ekranlar
Giriş (Y1), ana menü (Y2), oyuncu arama (O1), oyuncu nerede (O2), puan (L1), comp talebi (L2), günlük özet (R1),
masa haritası (C1, salt okunur), slot salonu (S1, salt okunur). Örnek görüntüler: `docs/ekran-goruntuleri/mobil/`.

## Klasörler
| Klasör | Ne yapar |
|---|---|
| `lib/api` | Taşıma (bearer, `X-Terminal`, `X-Correlation-Id`, problem+json) ve **üretilen** tipli istemci (`generated/`) |
| `lib/auth` | OIDC + PKCE (flutter_appauth), token yönetimi; yenileme token'ı Keystore/Keychain'de |
| `lib/access` | Sunucu menüsünden yetki (`<ekran>:<işlem>`) |
| `lib/features/<özellik>/{domain,data,presentation}` | Repository sözleşmesi / API uygulaması / ekran + ViewModel. Özellikler birbirini import etmez |
| `lib/core` | Özellikler arası sözleşmeler (ör. `LoyaltyPointsReader`); bağlama `lib/app/services.dart` |
| `lib/theme/sgt_tokens.dart` | Tüm tema değerleri (koyu/açık). Ortak token dosyası gelince bununla değiştirilir |
| `lib/l10n` | TR (varsayılan) / EN metinleri (ARB) |
| `tool/gen_api.dart` | `docs/api/openapi-v1.json` → `lib/api/generated/sgt_api.g.dart` |

## Komutlar
```bash
flutter pub get
flutter analyze
flutter test                                   # birim + widget + golden
flutter test --update-goldens test/goldens     # bilinçli görsel değişiklik (Linux'ta üretilir)
dart run tool/gen_api.dart                     # API sözleşmesi değişince
flutter run --dart-define=SGT_API_BASE=http://10.0.2.2:5067/ \
  --dart-define=SGT_AUTH_ISSUER=http://10.0.2.2:8080/realms/sgt
```

## Ayarlar (`--dart-define`)
| Ad | Açıklama |
|---|---|
| `SGT_API_BASE` | API adresi |
| `SGT_AUTH_ISSUER` | Keycloak realm adresi (`.../realms/sgt`) |
| `SGT_CLIENT_ID` | Varsayılan `sgt-mobile` (public, PKCE; dönüş `com.sentezgaming.sgt:/oauthredirect`) |
| `SGT_PRODUCTION` | Yalnızca HTTPS. Verilmezse release derlemede açık, debug'da kapalı |
| `SGT_PINNED_CERT` | Sabitlenecek sertifikanın asset yolu (PEM); verilirse yalnızca ona güvenilir |

Koda adres, secret veya gerçek veri yazılmaz. Android'de ekran görüntüsü engellidir (FLAG_SECURE), yedekleme kapalıdır.
Debug derlemesi yalnızca `10.0.2.2` ve `localhost` için şifresiz HTTP'ye izin verir.

## Android sürüm derlemesi (CI ortam değişkenleri, depoya yazılmaz)
| Ad | Açıklama |
|---|---|
| `SGT_API_HOST` | API host adı (ör. `sgt-api.ornek-casino.local`) |
| `SGT_AUTH_HOST` | Keycloak host adı (ör. `giris.ornek-casino.local`) |
| `SGT_PIN_SHA256_PRIMARY` | Sunucu sertifikasının SPKI SHA-256 özeti (base64). Örnek komut aşağıda |
| `SGT_PIN_SHA256_BACKUP` | Yedek anahtarın özeti (sertifika yenilemesi için zorunlu) |
| `SGT_PIN_EXPIRATION` | İsteğe bağlı `yyyy-MM-dd`; sonra pin denetimi devre dışı kalır |
| `SGT_ANDROID_CA_FILE` | İsteğe bağlı: casinonun iç CA'sı (PEM), güven çapası olarak eklenir |
| `SGT_ANDROID_KEYSTORE` | Sürüm imzası anahtar deposu yolu (CI secret dosyası) |
| `SGT_ANDROID_KEYSTORE_PASSWORD`, `SGT_ANDROID_KEY_ALIAS`, `SGT_ANDROID_KEY_PASSWORD` | İmza parolaları ve anahtar adı (CI secret) |

Pin'ler veya imza anahtarı verilmeden `flutter build apk --release` / `appbundle` kırılır; sürüm derlemesi debug anahtarıyla imzalanmaz.

```bash
# SPKI pin değeri (yer tutucu host):
openssl s_client -connect sgt-api.ornek-casino.local:443 </dev/null 2>/dev/null | openssl x509 -pubkey -noout \
  | openssl pkey -pubin -outform der | openssl dgst -sha256 -binary | base64
```

**Sabitleme kapsamı (ADR-0008):** API ve token iptali Dart istemcisinden gider ve `SGT_PINNED_CERT` ile sabitlenir (iki platform).
Android'de AppAuth token istekleri `network_security_config` ile sabitlenir. **iOS'ta AppAuth'un yerel token istekleri
henüz sabitlenmez** (`NSPinnedDomains` açık iş). Sistem tarayıcısındaki Keycloak giriş sayfası iki platformda da kapsam dışıdır.
