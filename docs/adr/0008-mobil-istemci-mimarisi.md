# ADR-0008: Mobil istemci mimarisi (durum yönetimi, token saklama, sertifika sabitleme)

- Tarih: 22.11.2026 · Durum: Kabul edildi · İlgili: Spec 014, ADR-0003

## Bağlam
S Plus mobil uygulaması (Flutter, iOS/Android) sahada casino müdürü, pit/slot yöneticisi ve host tarafından
kullanılır. Oyuncu ve tutar verisi gösterir; cihaz kaybolabilir, ortak kullanılabilir veya güvenilmeyen ağa bağlanabilir.
Windows ve web ile aynı API ve yetki modeli kullanılmalı (kararlar 2), kod kuralları K1/K2 geçerlidir.

## Karar

### 1. Durum yönetimi: ChangeNotifier + InheritedWidget, ek paket yok
- Her ekranın mantığı bir ViewModel'dedir (`ChangeNotifier`); ekran `ListenableBuilder` ile dinler.
  Veri durumu kapalı tip `LoadState` (Idle/Loading/Loaded/Failed) ile taşınır.
- Bağımlılıklar tek bileşim kökünde (`AppServices`) kurulur ve `InheritedWidget` (`ServicesScope`) ile verilir.
- Özellik klasörü `{data, domain, presentation}`: repository sözleşmesi domain'de, API uygulaması data'da.
  Özellikler birbirini import etmez; ortak ihtiyaç `lib/core` sözleşmesiyle (ör. `LoyaltyPointsReader`) çözülür
  ve uygulama düzeyinde bağlanır (K2). Mimari test (`test/architecture`) bunu denetler.
- Gerekçe: Flutter çerçevesinin kendi araçları yeterli; Provider/Riverpod/Bloc ek bağımlılık ve öğrenme yükü getirir.
  Ekran sayısı büyürse bu karar yeniden değerlendirilir.

### 2. Token saklama
- Giriş OIDC Authorization Code + PKCE (S256), sistem tarayıcısında (flutter_appauth). Uygulamada şifre alanı yok.
- Erişim token'ı yalnızca bellekte. Yenileme token'ı ve id token yalnızca `flutter_secure_storage`
  (Android Keystore, iOS Keychain `first_unlock_this_device`); Android yedeklemesi kapalı.
- `offline_access` istenmez; yenileme token'ı Keycloak SSO oturum sınırlarına (boşta 30 dk, en çok 10 sa) bağlıdır.
- Çıkışta: yerel depo silinir → yenileme token'ı Keycloak revocation ucunda iptal edilir → `endSession`.
  Ağ hatası yerel çıkışı engellemez. API 401 dönerse oturum kapanır.
- `sub` veya `tenant_id` olmayan token kabul edilmez (ADR-0003).

### 3. Sertifika sabitleme kapsamı
| Trafik | Android | iOS |
|---|---|---|
| API istekleri (Dart `http`) | `SGT_PINNED_CERT` PEM'i ile `SecurityContext(withTrustedRoots: false)` | Aynı (Dart TLS) |
| AppAuth token/yenileme (yerel ağ yığını) | `network_security_config` pin-set (SPKI SHA-256, birincil + yedek), derlemede üretilir | **Kapsam dışı**: AppAuth `URLSession` kullanır; `NSPinnedDomains` (Info.plist) ile eklenebilir, henüz yapılmadı |
| Token iptal isteği (Dart `http`) | Dart istemcisi (sabitlemeli) | Aynı |
| Keycloak giriş sayfası (sistem tarayıcısı) | Kapsam dışı (tarayıcı uygulamanın ağ yapılandırmasını kullanmaz) | Kapsam dışı (`ASWebAuthenticationSession`) |

- Sürüm derlemesi pin'ler verilmeden kırılır (Android). Pin değerleri ve hostlar depoda değil, CI ayarındadır.
- Sertifika yenilemesinde yedek pin önceden yayımlanır; aksi halde uygulama sunucuya bağlanamaz.

## Sonuçlar
- iOS'ta yerel ağ yığını için sabitleme ve iOS ekran görüntüsü engeli açık iştir.
- Sistem tarayıcısındaki giriş sayfası işletim sisteminin sertifika doğrulamasına güvenir; MFA bu riski azaltır.
- Durum yönetimi paketsiz olduğundan bağımlılık yüzeyi küçüktür; testlerde sahte taşıma (`ApiTransport`) yeterlidir.
