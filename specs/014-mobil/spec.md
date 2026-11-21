# Spec 014: S Plus mobil uygulama (A1 MVP)

Modül: istemci (`clients/mobile`, Flutter) · Ürün: S Plus · Durum: Uygulandı (21.11.2026) · Dal: `faz1/mobil-a1`
Ekranlar: Y1, Y2, O1, O2, L1, L2 (talep), C1, S1, R1 · Kararlar: `docs/kararlar.md` 2, 4, 9a · ADR-0003

## Amaç
Casino müdürü, pit/slot yöneticisi ve host sahada telefondan oyuncuyu bulur, nerede oynadığını ve puanını görür,
comp talep eder, günün özetini ve masa/slot durumunu izler. Uygulama veritabanına bağlanmaz; yalnızca `/api/v1`
kullanır ve Windows/web ile aynı yetki modelini izler.

## Kullanıcı senaryoları
1. Host telefondan giriş yapar (Keycloak sayfası, MFA dahil), menüde yalnızca yetkili olduğu ekranları görür.
2. Oyuncuyu numara/ad ile arar, "şu an nerede" sorusuna masa kodu ve başlangıç saatiyle yanıt alır.
3. Oyuncunun puan bakiyesini, seviyesini ve son hareketlerini görür; comp (puan harcama) talebi açar.
4. Müdür günün kasa, canlı oyun, slot ve loyalty özetini para birimi bazında görür, geçmiş bir günü seçer.
5. Pit/slot yöneticisi masa listesini (açık/kapalı, limitler, süren rating'ler) ve slot salonunu (bugünkü giren/kazanç) salt okunur izler.

## Kabul kriterleri
- **KK1 Giriş:** OIDC Authorization Code + PKCE (S256), sistem tarayıcısı (flutter_appauth), Keycloak realm `sgt`,
  istemci `sgt-mobile` (public, `com.sentezgaming.sgt:/oauthredirect`). Uygulama şifre alanı içermez.
- **KK2 Token:** Erişim token'ı yalnızca bellekte; yenileme token'ı yalnızca `flutter_secure_storage`
  (Android Keystore / iOS Keychain) içinde. `sub` veya `tenant_id` claim'i eksik token reddedilir, oturum açılmaz.
  Süresi dolmaya 30 sn kalan token istekten önce yenilenir; yenileme başarısızsa oturum kapanır ve giriş ekranı açılır.
  Çıkışta saklanan token silinir ve Keycloak oturumu kapatılır.
- **KK3 Menü:** Menü `GET /api/v1/identity/me/menu` yanıtından kurulur. Yanıtta olmayan ekran menüde görünmez ve
  rotası doğrudan açılsa da "yetkiniz yok" gösterir. Bir eylem düğmesi (ör. comp talebi) yalnızca ilgili eylem
  (`loyalty.comp:request`) menü yanıtında varsa görünür. Mobilde karşılığı olmayan yetkili ekranlar listelenmez.
- **KK4 İstek başlıkları:** Her istek `Authorization: Bearer`, `X-Terminal` (cihaza özgü rastgele kimlik, `M-` + 8 hex;
  cihaz adı veya kişisel bilgi değil) ve istek başına `X-Correlation-Id` (UUID) taşır.
- **KK5 Hatalar:** `application/problem+json` yanıtının başlığı ekranda gösterilir; 401 oturumu kapatır, 403 "yetkiniz yok"
  gösterir, ağ hatası "sunucuya ulaşılamadı" gösterir. Hiçbir hata kişisel veri içeren log satırı üretmez.
- **KK6 Oyuncu arama (O1):** En az 2 karakterle `GET /api/v1/players?q=` çağrılır; sonuçta oyuncu no, ad soyad, kart no,
  kısıtlı işareti görünür. 2 karakterden kısa aramada istek atılmaz.
- **KK7 Konum (O2):** Seçilen oyuncu için `GET /api/v1/floor/players/{id}/location`; 200 → masa kodu + başlangıç saati
  (KKTC saati), 204 → "şu an oyunda değil".
- **KK8 Puan (L1):** `GET /api/v1/loyalty/players/{id}`; bakiye, toplam kazanılan, seviye ve son hareketler (tür, puan, zaman).
- **KK9 Comp talebi (L2):** Puan (pozitif tam sayı), açıklama ve gerekçe kodu (1–50 karakter, büyük harfe çevrilir) zorunlu.
  `POST /api/v1/loyalty/comps` istemcinin ürettiği `requestId` ile gönderilir; ağ hatasından sonra aynı bilgilerle
  tekrar gönderimde aynı `requestId` kullanılır (çift talep olmaz), bilgi değişirse yeni `requestId` üretilir.
- **KK10 Günlük özet (R1):** `GET /api/v1/reports/daily?date=`; tarih seçilmezse bugün. Kasa (vardiya sayıları, para birimi
  bazında çip satış/bozdurma), canlı oyun (biten rating, teorik kazanç, casino sonucu), slot (giren, kazanç), loyalty (kazanılan/harcanan).
  Boş bölüm "veri yok" gösterir.
- **KK11 Masa haritası (C1):** `GET /api/v1/floor/tables` (+ `live.rating:view` varsa `GET /api/v1/floor/ratings/running`);
  masa kodu, oyun, para birimi, min–max, açık/kapalı, o masadaki süren rating sayısı. Salt okunur.
- **KK12 Slot haritası (S1):** `GET /api/v1/floor/slots`; alana göre gruplu, varlık no, son okuma, bugünkü giren ve kazanç. Salt okunur.
- **KK13 Para:** Tutarlar `Decimal` ile taşınır ve gösterilir; `double` kullanılmaz. Para birimi kodu her tutarın yanında yazılır.
- **KK14 Tipli API istemcisi:** DTO'lar ve istek fonksiyonları `docs/api/openapi-v1.json` dosyasından `tool/gen_api.dart` ile üretilir;
  elle DTO yazılmaz. Üretilen dosya sözleşmeyle uyumsuzsa test kırılır.
- **KK15 Dil:** Türkçe (varsayılan) ve İngilizce; tüm ekran metinleri ARB dosyalarındadır.
- **KK16 Tasarım (9a):** Kurumsal, yoğun liste/tablo düzeni; köşe yarıçapı ≤ 3 px; hap düğme, büyük yuvarlak kart ve süs
  animasyonu yok; koyu ve açık tema. Tüm tema değerleri tek dosyada (`lib/theme/sgt_tokens.dart`).
- **KK17 Güvenlik (MASVS temel):** Kodda secret yok (adresler `--dart-define`); üretimde HTTPS zorunlu; sertifika sabitleme
  kancası (`SGT_PINNED_CERT` asset'i verilirse yalnızca o sertifikaya güvenilir); Android'de ekran görüntüsü/son uygulamalar
  önizlemesi engellenir (FLAG_SECURE).
- **KK18 Doğrulama:** `flutter analyze` temiz; `flutter test` yeşil; her ekranın koyu ve açık golden görüntüsü (sentetik veri)
  `docs/ekran-goruntuleri/mobil/` altına kopyalanır.

## Kapsam dışı / API eksikleri (uydurulmadı)
| Ekran | Eksik uç | Not |
|---|---|---|
| O4 Oyuncu geçmişi | `GET /api/v1/players/{id}/history` (ziyaret, oyun, kasa) | Ekran yapılmadı |
| L2 Comp onayı | `GET /api/v1/loyalty/comps?status=pending` (bekleyen talepler) | Karar ucu (`POST .../decision`) var ama onaylanacak talep listelenemiyor; istemci metodu hazır, ekran yok |
| L3 Host portföyü | `GET /api/v1/loyalty/hosts/me/players` | Ekran yapılmadı |
| O2 Konum (yalnız konum yetkisi) | `GET /api/v1/players?q=` `player.search:view` ister | `player.locator` yetkili ama aramasız kullanıcı oyuncu bulamaz; konum-kapsamlı arama ucu veya yetki istisnası gerekir |
| C1/S1 harita | Masa/makine konum (x,y, pit) bilgisi | Harita yerine alan/pit gruplu liste gösterilir |

## Bağımlılıklar
Spec 001 (yetki/menü), 005 (oyuncu), 007 (canlı oyun), 008 (loyalty), 009 (slot), 011 (günlük özet).
