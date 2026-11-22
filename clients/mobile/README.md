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
| `lib/features/<özellik>` | Ekran + ViewModel (ChangeNotifier) + repository |
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
| `SGT_PRODUCTION` | `true` ise yalnızca HTTPS |
| `SGT_PINNED_CERT` | Sabitlenecek sertifikanın asset yolu (PEM); verilirse yalnızca ona güvenilir |

Koda adres, secret veya gerçek veri yazılmaz. Android'de ekran görüntüsü engellidir (FLAG_SECURE), yedekleme kapalıdır.
Android emülatörde yerel `http` için ayrıca network security config gerekir (yalnızca geliştirme).
