# SGT kurulum rehberi (yerel geliştirme ve pilot)

- Tarih: 26.10.2026 · İlgili kararlar: ADR-0002 (veritabanı), ADR-0003 (kimlik doğrulama), ADR-0005 (kişisel veri)
- Dosyalar: `deploy/docker-compose.yml`, `deploy/.env.example`, `deploy/api.Dockerfile`, `deploy/keycloak/sgt-realm.json`

Bu belge iki bölümdür. **Bölüm A** proje sahibi için sade bir özettir. **Bölüm B** kurulumu yapacak
teknisyen içindir; komutlar olduğu gibi kopyalanıp çalıştırılabilir.

---

## Bölüm A — Proje sahibi için özet

SGT'yi çalıştırmak için bilgisayarda (veya sunucuda) üç parça birlikte çalışır:

| Parça | Ne işe yarar | Benzetme |
|---|---|---|
| **PostgreSQL** | Tüm kayıtların tutulduğu veritabanı | Arşiv dolabı |
| **Keycloak** | Giriş sistemi: kullanıcı adı, şifre ve telefondaki tek kullanımlık kod (MFA) | Kapıdaki güvenlik görevlisi |
| **SGT API** | Casino iş kuralları (oyuncu, kasa, yetki paneli...) | Ofisin kendisi |

Bu üç parça **Docker** denen bir programla tek komutla açılır ve kapanır. Docker, her parçayı kendi
"kutusunda" çalıştırır; bilgisayara ayrı ayrı program kurmak gerekmez.

Bilmeniz gereken önemli noktalar:

1. **Keycloak "kim olduğunu" söyler, SGT "ne yapabileceğini" söyler.** Bir kişinin giriş yapabilmesi için
   Keycloak'ta hesabı olmalıdır; hangi ekranı görüp hangi işlemi yapacağı ise SGT yetki panelinden verilir.
2. **Geliştirme şifreleri yalnızca deneme içindir.** `deploy/.env.example` dosyasındaki şifrelerin
   yanında "Dev-Only / dev_only" yazar. Pilot veya gerçek casino kurulumunda hepsi değiştirilir.
3. **Gerçek şifreler ve anahtarlar asla koda/depoya girmez.** Üretimde bir "secret manager"
   (şifre kasası) kullanılır. Bkz. Bölüm B-6.
4. **İki özel anahtar** oyuncu kişisel verisini şifreler. Bu anahtar kaybolursa şifreli veri **geri
   okunamaz**; çalınırsa veri açığa çıkabilir. Bu yüzden anahtar hem gizli tutulur hem de güvenli bir
   yerde yedeklenir (veritabanı yedeğinden **ayrı**).
5. **Yedek her gece alınır** ve ayda bir "geri yükleme denemesi" yapılır. Denenmemiş yedek, yedek sayılmaz.
6. **Telefonla doğrulama (MFA)** pilot ve üretimde herkes için zorunludur. Geliştirmede kolaylık için
   isteğe bağlı bırakılabilir (ayar: `SGT_OTP_REQUIREMENT`).

---

## Bölüm B — Teknisyen için kurulum

### B-1. Gerekenler

- Docker Engine 24+ ve Docker Compose v2 (`docker compose version` çalışmalı).
  Windows/Mac için Docker Desktop (lisans notu için B-8'e bakın).
- API'yi Docker dışında çalıştırmak isterseniz: .NET 10 SDK.
- Boş portlar: `5432` (PostgreSQL), `8080` (Keycloak), `5080` (API). Doluysa `deploy/.env` içinden değiştirin.
- `openssl` (anahtar üretmek için; Git for Windows ile birlikte gelir).

Tüm komutlar **repo kök klasöründen** çalıştırılır.

### B-2. İlk kurulum (geliştirme)

```bash
# 1) Ortam dosyasını oluşturun (deploy/.env repoya girmez, .gitignore'dadır)
cp deploy/.env.example deploy/.env

# 2) PostgreSQL + Keycloak'ı başlatın
docker compose -f deploy/docker-compose.yml --env-file deploy/.env up -d

# 3) Durumu izleyin; ikisi de "healthy" olana kadar bekleyin (Keycloak ilk açılışta ~30-60 sn)
docker compose -f deploy/docker-compose.yml --env-file deploy/.env ps
```

İlk açılışta otomatik olarak:
- PostgreSQL'de `sgt_dev` (SGT) ve `keycloak` (giriş sistemi) veritabanları açılır
  (`deploy/postgres/init/01-keycloak-db.sh`).
- Keycloak `deploy/keycloak/sgt-realm.json` dosyasından `sgt` alanını (realm) içe aktarır:
  istemciler, giriş akışı, iki sentetik kullanıcı.

### B-3. SGT API'yi çalıştırmak (iki yol)

**Yol 1 — Docker ile (önerilen, kurulum testi için):**

```bash
docker compose -f deploy/docker-compose.yml --env-file deploy/.env --profile api up -d --build
curl http://localhost:5080/health          # {"status":"ok",...}
```

**Yol 2 — Geliştirici bilgisayarında doğrudan (kod yazarken):**

```bash
# deploy/.env içinde POSTGRES_PORT=5432 ise appsettings.Development.json hiç değişmeden çalışır
dotnet run --project src/Bootstrapper/Sgt.Api
```

**Veritabanı tabloları (migration):** API `Development` ortamında açılırken tüm modüllerin
tablolarını kendisi kurar/günceller ve `DevelopmentSeed` ayarındaki süper admin yetkisini ekler.
Ayrı bir komut gerekmez. İlk açılışta logda `__ef_migrations ... Failed executing` satırları görmek
normaldir (tablo henüz yoktur, hemen ardından oluşturulur).

`Production` ortamında API migration **çalıştırmaz** (bkz. B-6.4).

### B-4. Giriş yapmak ve denemek

| Ne | Adres | Kullanıcı | Şifre (DEV ONLY) |
|---|---|---|---|
| Keycloak yönetim konsolu | http://localhost:8080/admin | `kcadmin` | `Kc-Admin-Dev-Only-1` |
| SGT süper admin | http://localhost:8080/realms/sgt/account | `superadmin` | `Superadmin-Dev-Only-1` |
| SGT kasiyer (sentetik) | http://localhost:8080/realms/sgt/account | `kasiyer1` | `Kasiyer1-Dev-Only-1` |

Şifreler `deploy/.env` içinden gelir; yukarıdakiler `.env.example`'daki **yalnızca geliştirme**
değerleridir.

Realm içeriği:

- Realm: `sgt` · Issuer: `http://localhost:8080/realms/sgt`
- `sgt-web`: public istemci, Authorization Code + PKCE (S256), yönlendirme `http://localhost:3000/*`.
- `sgt-api`: token hedefi (audience). Access token'da `aud` içinde `sgt-api` bulunur.
- Access token'da `sub` (kullanıcı GUID) ve `tenant_id` (müşteri GUID) bulunur. `tenant_id`, kullanıcı
  özniteliğidir; protocol mapper ile token'a yazılır. Kullanıcı profilinde yalnızca yöneticinin
  görebileceği/değiştirebileceği alan olarak tanımlıdır.
- Kullanıcı kimlikleri sabittir:
  - `superadmin` → `00000000-0000-0000-0000-000000000001` (SGT `DevelopmentSeed:SuperAdminUserId` ile aynı)
  - `kasiyer1` → `00000000-0000-0000-0000-000000000002`
  - ikisinin de `tenant_id` = `00000000-0000-0000-0000-0000000000a1`
- Şifre politikası: en az 12 karakter, büyük/küçük harf, rakam, kullanıcı adı olamaz. 5 hatalı denemede
  geçici kilit.

**Komut satırından token alıp API'yi denemek (yalnızca geliştirme, `SGT_DEV_DIRECT_GRANT=true`):**

```bash
TOKEN=$(curl -s -d grant_type=password -d client_id=sgt-web \
  -d username=superadmin -d password=Superadmin-Dev-Only-1 -d scope=openid \
  http://localhost:8080/realms/sgt/protocol/openid-connect/token | python3 -c 'import sys,json;print(json.load(sys.stdin)["access_token"])')

curl -H "Authorization: Bearer $TOKEN" -H "X-Terminal: dev-pc" http://localhost:5080/api/v1/identity/me/menu
```

Beklenen: süper admin için ekran listesi (200). Token yoksa 401.

**kasiyer1 neden 403 alır?** Keycloak'ta hesabı vardır ama SGT tarafında henüz kayıtlı ve yetkili
değildir. Süper admin, SGT yetki panelinden (`POST /api/v1/identity/users`, `userId` =
`00000000-0000-0000-0000-000000000002`) kasiyeri kaydedip rol atadığında erişim açılır.
Yeni bir personel eklerken kural: **Keycloak kullanıcı ID'si = SGT kullanıcı ID'si**.

### B-5. MFA (telefonla tek kullanımlık kod, OTP)

Giriş akışı `sgt browser`: kullanıcı adı + şifre, ardından OTP adımı. OTP adımının davranışı
`deploy/.env` içindeki `SGT_OTP_REQUIREMENT` ile seçilir:

| Değer | Davranış | Nerede |
|---|---|---|
| `CONDITIONAL` | Yalnızca OTP kurmuş kullanıcıdan kod istenir | Geliştirme (`.env.example` varsayılanı) |
| `REQUIRED` | Herkes ilk girişte telefonuna (Google/Microsoft Authenticator, FreeOTP) OTP kurar, sonra her girişte kod girer | **Pilot ve üretim (zorunlu)** |

Not: `.env`'de değişken hiç yoksa compose dosyası güvenli tarafı (`REQUIRED`) seçer.

**Önemli — bu ayar yalnızca realm ilk içe aktarılırken okunur.** Realm zaten varsa Keycloak dosyayı
tekrar okumaz. Mevcut kurulumda değiştirmek için:
- Yönetim konsolu → `sgt` realm → **Authentication** → `sgt browser` → `sgt browser otp` satırı →
  **Required** (veya **Conditional**) seçin. Değişiklik hemen geçerlidir.
- Ya da (yalnızca geliştirme, tüm Keycloak verisi silinir) B-7'deki "sıfırdan başlatma" yapılır.

`SGT_DEV_DIRECT_GRANT=true` komut satırından şifreyle token almayı açar; bu yol OTP adımını atlayabilir.
**Pilot ve üretimde mutlaka `false`** olmalıdır.

### B-6. Pilot / üretim: secret'lar ve güvenlik

#### B-6.1 Secret'lar nerede durur

- Gerçek şifre ve anahtarlar **secret manager**'da durur (ör. Azure Key Vault, HashiCorp Vault,
  Windows'ta en azından yalnızca servis hesabının okuyabildiği şifreli bir depo). Repoda, Docker
  imajında, e-postada veya sohbet/yapay zeka bağlamında **bulunmaz**.
- Docker imajı geliştirme anahtarlarını içermez (`appsettings.Development.json` imajdan silinir).
  Tüm ayarlar ortam değişkeniyle verilir:

| Ayar (ortam değişkeni) | Ne |
|---|---|
| `ConnectionStrings__Sgt` | Veritabanı bağlantısı (şifre içerir) |
| `Authentication__Authority` | Keycloak realm adresi, ör. `https://giris.ornek-casino.com/realms/sgt` |
| `Authentication__Audience` | `sgt-api` |
| `Security__PersonalDataKey` | Kişisel veri şifreleme anahtarı (ADR-0005) |
| `Security__BlindIndexKey` | Şifreli alanda arama anahtarı (ADR-0005) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

#### B-6.2 Anahtar üretimi

Her kurulum (her casino) için **ayrı** anahtar üretilir; iki anahtar birbirinden farklı olmalıdır:

```bash
openssl rand -base64 32    # -> Security:PersonalDataKey
openssl rand -base64 32    # -> Security:BlindIndexKey
```

Çıktıyı doğrudan secret manager'a yapıştırın; dosyaya, ekran görüntüsüne veya loga yazmayın.
Anahtarların bir kopyası, veritabanı yedeğinden **ayrı** bir güvenli yerde (ör. kasadaki şifreli USB
veya ikinci secret manager) saklanır. Anahtar kaybı = şifreli kişisel verinin kalıcı kaybı.
Geliştirmedeki anahtarlar (`appsettings.Development.json`, `.env.example`) herkesçe bilinir; pilot ve
üretimde **asla** kullanılmaz.

Diğer şifreler için (DB, Keycloak yöneticisi): `openssl rand -base64 24`.

#### B-6.3 Keycloak üretim modu

- `start-dev` yerine `start` kullanılır; HTTPS zorunludur (sertifika veya önünde TLS sonlandıran bir
  ters vekil/reverse proxy). `KC_HOSTNAME=https://giris.<alan-adi>`.
- `KC_BOOTSTRAP_ADMIN_*` yalnızca ilk açılış içindir; ilk girişten sonra kalıcı, MFA'lı bir yönetici
  hesabı oluşturup geçici hesabı silin.
- `SGT_OTP_REQUIREMENT=REQUIRED`, `SGT_DEV_DIRECT_GRANT=false`.
- Sentetik kullanıcılar (`superadmin`, `kasiyer1`) ve onların şifreleri üretimde kullanılmaz; gerçek
  personel hesapları yönetim konsolundan açılır, `tenant_id` özniteliği girilir.
- `sgt-web` yönlendirme adresi (`http://localhost:3000/*`) gerçek web paneli adresine göre değiştirilir.

#### B-6.4 Üretimde migration

`Production`'da API tabloları kendisi kurmaz; teknik lider her sürümde migration'ı kontrollü çalıştırır.
Her modül için (Identity, Audit, Configuration, CasinoPlayer, Finance, GamingFloor, Loyalty):

```bash
dotnet tool restore
dotnet ef database update \
  -p src/Modules/Identity/Sgt.Modules.Identity.Infrastructure \
  --connection "<secret manager'dan bağlantı>"
```

Önerilen: önce `dotnet ef migrations script --idempotent -p ... -o identity.sql` ile SQL üretip
gözden geçirmek, sonra uygulamak. Migration öncesi mutlaka yedek alınır (B-7).

### B-7. Yedekleme ve geri yükleme

**Ne yedeklenir:** `sgt_dev` (üretimde SGT veritabanı adı) ve `keycloak` veritabanları. Şifreleme
anahtarları ayrı yedeklenir (B-6.2).

**Elle yedek:**

```bash
mkdir -p yedek
docker compose -f deploy/docker-compose.yml --env-file deploy/.env exec -T postgres \
  pg_dump -U sgt -d sgt_dev -Fc > yedek/sgt_$(date +%Y%m%d_%H%M).dump
docker compose -f deploy/docker-compose.yml --env-file deploy/.env exec -T postgres \
  pg_dump -U sgt -d keycloak -Fc > yedek/keycloak_$(date +%Y%m%d_%H%M).dump
```

(`-Fc` sıkıştırılmış özel biçimdir; `pg_restore` ile geri yüklenir.)

**Zamanlama (Linux, cron — her gece 02:30, 30 gün saklama):**

```cron
30 2 * * * cd /opt/sgt && docker compose -f deploy/docker-compose.yml --env-file deploy/.env exec -T postgres pg_dump -U sgt -d sgt_dev -Fc > /yedek/sgt_$(date +\%Y\%m\%d).dump && find /yedek -name '*.dump' -mtime +30 -delete
```

Windows'ta aynı komut bir `.ps1` dosyasına yazılıp **Görev Zamanlayıcı** (Task Scheduler) ile her gece
çalıştırılır.

Kurallar:
- Yedek aynı diskte kalmaz; günlük olarak ikinci bir yere (ayrı disk/NAS ve bina dışı) kopyalanır.
- Yedek dosyaları kişisel veri içerir: şifreli diskte, erişimi kısıtlı tutulur.
- Üretimde ek olarak PostgreSQL WAL arşivleme (anlık geri dönüş) değerlendirilir.
- **Ayda bir geri yükleme denemesi** boş bir veritabanına yapılır:

```bash
docker compose -f deploy/docker-compose.yml --env-file deploy/.env exec -T postgres createdb -U sgt sgt_geri_yukleme_testi
docker compose -f deploy/docker-compose.yml --env-file deploy/.env exec -T postgres \
  pg_restore -U sgt -d sgt_geri_yukleme_testi < yedek/sgt_YYYYMMDD_HHMM.dump
```

### B-8. Windows sunucu notları

- **Docker Desktop** ticari kullanımda (250+ çalışan veya yıllık gelir eşiği üstü) ücretli lisans ister
  ve Windows Server'da desteklenmez. Pilot için önerilen: Windows Server üzerinde bir **Linux sanal
  makine** (Hyper-V, Ubuntu LTS) açıp bu belgedeki Docker kurulumunu orada yapmak.
- Docker'sız (Windows servisleri ile) kurulum mümkündür:
  - PostgreSQL 16: resmi Windows kurulum paketi (Windows servisi olarak gelir).
  - Keycloak 26.7.5: Java 21 (OpenJDK) + `bin\kc.bat start ...`; Windows servisi olarak çalıştırmak
    için servis sarmalayıcı (ör. WinSW) gerekir. `sgt-realm.json` yönetim konsolundan
    "Realm oluştur → dosya seç" ile içe aktarılır (bu yolda `${...}` değişkenleri çözülmez; şifreler
    elle girilir).
  - SGT API: `dotnet publish src/Bootstrapper/Sgt.Api -c Release -o C:\sgt\api` ve bir servis
    sarmalayıcı ile servis olarak çalıştırma. Ortam değişkenleri servis hesabına tanımlanır.
- Windows Güvenlik Duvarı: dışarıya yalnızca HTTPS (443) açılır. PostgreSQL (5432) ve Keycloak yönetim
  portu (9000) dışarı açılmaz. Compose dosyası portları zaten yalnızca `127.0.0.1`'e bağlar.
- Saat dilimi ve saat eşitlemesi (NTP) açık olmalıdır; token süreleri saate bağlıdır.
- Satır sonları: `deploy/postgres/init/01-keycloak-db.sh` Windows'ta düzenlenirse LF olarak kalmalıdır
  (CRLF olursa PostgreSQL ilk açılışta hata verir).

### B-9. Günlük komutlar

```bash
# Durdur (veri korunur)
docker compose -f deploy/docker-compose.yml --env-file deploy/.env --profile api down

# Loglar
docker compose -f deploy/docker-compose.yml --env-file deploy/.env logs -f keycloak
docker compose -f deploy/docker-compose.yml --env-file deploy/.env logs -f api

# API'yi yeni kodla yeniden derle ve başlat
docker compose -f deploy/docker-compose.yml --env-file deploy/.env --profile api up -d --build api

# SIFIRDAN BAŞLAT — TÜM VERİ SİLİNİR (yalnızca geliştirme!)
docker compose -f deploy/docker-compose.yml --env-file deploy/.env --profile api down -v
```

### B-10. Sık sorunlar

| Belirti | Neden / çözüm |
|---|---|
| `address already in use` (5432/8080) | Port dolu. `deploy/.env` içinde `POSTGRES_PORT` / `KEYCLOAK_PORT` değiştirin. (API'yi Docker dışında çalıştırıyorsanız bağlantı adresindeki portu da değiştirin.) |
| `.env eksik` hatası | `cp deploy/.env.example deploy/.env` yapılmamış. |
| API 401 veriyor, token doğru görünüyor | Token'daki `iss` ile API'nin `Authentication:Authority` adresi aynı realm'i göstermeli; `aud` içinde `sgt-api` olmalı. Tarayıcıdan farklı bir adresle Keycloak'a gidiliyorsa `KC_HOSTNAME_URL` o adres yapılır. |
| API 403 veriyor | Kimlik doğru, ama SGT yetki panelinde kullanıcının yetkisi yok (B-4). |
| Realm değişikliği yansımadı | Keycloak var olan realm'i tekrar içe aktarmaz (B-5). |
| API logunda `libgssapi_krb5.so.2` uyarısı | Zararsız; veritabanı sürücüsü Kerberos kütüphanesini arar, bulamayınca şifreyle devam eder. |
| PostgreSQL ilk açılışta `KEYCLOAK_DB_PASSWORD` hatası | `.env`'de değer boş. Veri klasörü oluştuysa `down -v` ile sıfırlayın (geliştirme). |

## Güncelleme: veritabanı şeması tek komutla (ADR-0007)

Elle modül modül `dotnet ef database update` çalıştırmak yerine, yedekten sonra şu komut yeterlidir:

```bash
dotnet Sgt.Api.dll --migrate        # tüm modüllerin şemasını günceller ve çıkar
# Docker: docker compose run --rm api --migrate
```

Windows sunucuda API, "SGT API" adlı Windows servisi olarak kurulabilir:
`sc.exe create "SGT API" binPath= "C:\sgt\Sgt.Api.exe"` (ortam değişkenleri sistem düzeyinde tanımlanır).
