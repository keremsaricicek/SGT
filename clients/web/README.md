# SGT Web Yönetim Paneli

Next.js (App Router) + TypeScript (strict) + React ile yazılmış yönetim paneli. API ile yalnızca
`docs/api/openapi-v1.json` sözleşmesi üzerinden konuşur (ADR-0006); giriş Keycloak'a OIDC
Authorization Code + PKCE ile yapılır (ADR-0003).

## Gereksinimler

- Node.js 22 (22.12 veya üstü) ve npm
- Çalışan SGT API'si (`dotnet run --project src/Bootstrapper/Sgt.Api`)
- Keycloak'ta bu panel için bir **public client** (secret yok):
  - Standard flow açık, PKCE yöntemi `S256`
  - Valid redirect URI: `http://localhost:3000/auth/callback`
  - Post logout redirect URI ve Web origins: `http://localhost:3000`
  - Token'da `sub` ve `tenant_id` alanları olmalı (ADR-0003)

## Kurulum ve çalıştırma

```bash
cd clients/web
npm ci                      # bağımlılıkları package-lock.json'daki sabit sürümlerle kurar
cp .env.example .env.local  # değerleri kendi ortamınıza göre düzenleyin
npm run dev                 # geliştirme sunucusu: http://localhost:3000
```

Üretim derlemesi:

```bash
npm run build
npm start
```

## Yapılandırma (ortam değişkenleri)

| Değişken | Açıklama |
|---|---|
| `NEXT_PUBLIC_OIDC_AUTHORITY` | Keycloak realm adresi, ör. `http://localhost:8080/realms/sgt` |
| `NEXT_PUBLIC_OIDC_CLIENT_ID` | Keycloak public client kimliği |
| `NEXT_PUBLIC_OIDC_SCOPE` | İsteğe bağlı, varsayılan `openid profile` |
| `NEXT_PUBLIC_API_BASE_URL` | API kök adresi; boşsa istekler aynı kökene gider |
| `NEXT_PUBLIC_DEFAULT_TERMINAL` | İsteğe bağlı; tarayıcıda kayıtlı terminal adı yoksa kullanılır |
| `SGT_API_PROXY_TARGET` | İsteğe bağlı (yalnızca sunucu); `/api/v1/*` isteklerini Next üzerinden API'ye aktarır |

`NEXT_PUBLIC_*` değerleri derleme sırasında koda gömülür ve tarayıcıya gider; **secret yazılmaz**.
API şu an CORS açmıyor; panel ile API farklı adreslerdeyse ya API'de CORS açılmalı ya da
`SGT_API_PROXY_TARGET` kullanılıp `NEXT_PUBLIC_API_BASE_URL` boş bırakılmalıdır.

## Komutlar

| Komut | Ne yapar |
|---|---|
| `npm run dev` | Geliştirme sunucusu |
| `npm run build` | Üretim derlemesi |
| `npm run lint` | ESLint (uyarı da hata sayılır; feature sınırları da denetlenir) |
| `npm run typecheck` | TypeScript strict tip kontrolü |
| `npm test` | Vitest birim testleri (servisler ve hook'lar, sahte fetch ile) |
| `npm run gen:api` | `docs/api/openapi-v1.json`'dan tipleri yeniden üretir |

API sözleşmesi değişince `npm run gen:api` çalıştırılır ve `src/shared/api/generated/schema.d.ts`
commit edilir. CI, üretilen dosyanın sözleşmeyle aynı olduğunu kontrol eder.

## Nasıl çalışır

- **İstek başlıkları:** Her API isteği `Authorization: Bearer <token>`, `X-Terminal` ve
  `X-Correlation-Id` taşır. Terminal adı üst çubuktan değiştirilebilir ve tarayıcıda (localStorage)
  saklanır. Her kullanıcı eylemi (ekran yükleme, kaydet düğmesi) yeni bir correlation id üretir.
- **Menü ve yetki:** Menü `GET /api/v1/identity/me/menu` yanıtından oluşur; yalnızca görme yetkisi olan
  ekranlar görünür. İşlem düğmeleri yalnızca ilgili eylem menü öğesinde varsa gösterilir.
  Son karar her zaman sunucudadır; sunucu reddederse hata ekranda gösterilir.
- **Gerekçe kodu:** Her değişiklik formunda zorunlu gerekçe kodu alanı vardır (ör. `ROLE_SETUP`).
- **Hatalar:** API'nin döndürdüğü problem ayrıntısı (başlık/ayrıntı) ve işlem kimliği kullanıcıya gösterilir.
- **Dil:** Türkçe varsayılan, İngilizce seçilebilir (`src/shared/i18n`).

## Ekranlar

| Sayfa | Ekran kodu | API |
|---|---|---|
| `/yetki/roller`, `/yetki/roller/[id]` | `admin.roles` | roller, rol yetkisi ver/al, varsayılan roller |
| `/yetki/kullanicilar` | `admin.users` | kullanıcı listesi, kayıt, pasifleştir/aktifleştir |
| `/yetki/kullanicilar/[id]` | `admin.roles` | rol ata/kaldır, kişiye yetki ekle/çıkar |
| `/yetki/gecmis` | `admin.permission_history` | yetki değişiklik geçmişi |
| `/oyuncular`, `/oyuncular/[id]` | `player.search`, `player.profile`, `player.restrictions` | arama, kayıt, kart, kısıtlama |
| `/kur` | `admin.settings` | güncel kurlar, kur girişi |
| `/raporlar/gunluk` | `report.daily` | günlük özet |
| `/denetim` | `admin.audit` | denetim kayıtları |
| `/entegrasyon` | `admin.integrations` | dead-letter listesi, tekrar işle |

## Klasör yapısı

```text
src/
  app/                    yalnızca yönlendirme; feature'ları genel girişlerinden (index.ts) kullanır
  features/<feature>/     api/ (servis + tipler), hooks/, components/, index.ts
  shared/                 api istemcisi, kimlik doğrulama, menü/yetki, i18n, ortak UI
  test/                   test yardımcıları (sahte fetch, sağlayıcılar)
```

Feature'lar birbirinin iç koduna erişmez; ortak kod `src/shared` altındadır (docs/kod-kurallari.md §4).
Bu kural ESLint ile denetlenir.

## Notlar

- OpenAPI sözleşmesinde yanıt şemaları yok; yanıt tipleri her feature'ın `api/` klasöründe
  backend kayıtlarından elle yazılmıştır. Backend uçlara `Produces<T>()` eklerse bu tipler üretilebilir.
- Testlerde yalnızca sentetik veri vardır.
