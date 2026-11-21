# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# SGT (Sentez Gaming Technologies)

Kısa tutulur; ayrıntı `docs/` altında. Çelişkide öncelik: `.specify/memory/constitution.md` → `docs/kararlar.md` →
bu dosya → diğer `docs/` → `docs/fizibilite-raporu.pdf` (en eski).

KKTC casino/otelleri için ayrı satılan ürün ailesi: **S Plus** (casino çekirdeği, tek başına çalışır),
**S Slot** (slot verisi, NEXIO'nun yerine), **S Vista** (otel). Modüller lisansla açılır. Para birimi TRY/EUR/GBP/USD, arayüz TR/EN.

**Durum ve yol haritası:** `docs/SGT-tam-plan.pdf` (aşama A–I, iş paketleri, demolar). Çekirdek + S Plus MVP hazır;
sıradaki iş A aşaması (mobil, demo ortamı). Çalışma dalı `faz0/proje-paketi`; `main`e doğrudan iş yapılmaz, PR istenmeden açılmaz.

## Proje sahibiyle çalışma

- Sahip kod bilmez: sade, kısa Türkçe. Token tasarrufu en önemli konu: ara rapor yazılmaz, "durum ne" denince kısa rapor.
- Teknik koordinatör sensin: kararları kendin ver, yalnızca gerçekten sahibe ait kararları sor (açıklar `docs/kararlar.md` sonunda).
- Her aşama sonunda denenebilir demo; UI işi ekran görüntüsüyle (Playwright) doğrulanmadan bitmiş sayılmaz.

## Komutlar

```bash
dotnet build Sgt.slnx                                    # uyarı = hata
dotnet test Sgt.slnx                                     # tüm sunucu testleri
dotnet test tests/Architecture/Sgt.Tests.Architecture.csproj
dotnet test tests/Modules/Finance/Sgt.Modules.Finance.Tests --filter "FullyQualifiedName~OpenShift"   # tek test/sınıf
dotnet run --project src/Bootstrapper/Sgt.Api            # lokal API (Development: migrate + seed); /health
dotnet run --project src/Bootstrapper/Sgt.Api -- --migrate   # yalnızca migration (üretim yolu, ADR-0007)
build/new-module.sh <Modül>                              # modül iskeleti; sonra ModuleCatalog'a ekle
dotnet ef migrations add <Ad> -p src/Modules/<M>/Sgt.Modules.<M>.Infrastructure -o Persistence/Migrations
SGT_UPDATE_OPENAPI=1 dotnet test tests/Api/Sgt.Api.Tests --filter OpenApiContractTests   # bilinçli API değişikliği
```
- Entegrasyon/API testleri yerel PostgreSQL ister (`SGT_TEST_DB`, varsayılan `Host=localhost;Username=sgt;Password=sgt_dev_only`);
  bulut oturumunda `.claude/hooks/session-start.sh` kurar. Tam ortam: `deploy/docker-compose.yml` (Postgres + Keycloak), bkz. `docs/kurulum.md`.
- Web (`clients/web`, Next.js 16): `npm run lint|typecheck|test|build`; API tipleri `npm run gen:api` (OpenAPI'den üretilir, elle yazılmaz).
- Windows (`clients/windows/Sgt.Windows.slnx`): `Sgt.Windows.Core(.Tests)` Linux'ta derlenir/test edilir; `Sgt.Windows.App` (WPF) yalnızca Windows/CI.
- Mobil (`clients/mobile`, Flutter): yarım; `faz1/mobil` dalında.

## Mimari (büyük resim)

Modüler monolit, .NET 10 minimal API, PostgreSQL 16 + EF Core 10. Tek host: `src/Bootstrapper/Sgt.Api/Program.cs`
her modülü `Add<Modül>Module(...)` + `Map<Modül>Endpoints()` ile bağlar.

- **Modül = 5 proje** (`src/Modules/<M>/Sgt.Modules.<M>.*`): `Contracts` (dışa açık arayüz, DTO, integration event) ·
  `Domain` (aggregate, value object; altyapı yok) · `Application` (özellik klasörlerinde dikey dilim: komut/sorgu + handler + validator,
  ör. `Finance.Application/Cage/OpenShift.cs`) · `Infrastructure` (kendi şeması + DbContext, row sınıfları domain'den ayrı, aggregate `Restore(...)` ile kurulur) ·
  `Api` (`<M>Endpoints.cs`, `/api/v1/...`).
- **Modüller arası:** yalnızca `*.Contracts`. Durum değiştiren iletişim integration event + modül başına outbox (`OutboxProcessor`,
  backoff, 10 denemede dead-letter); tüketim `EfInbox` ile idempotent. Anlık okuma Contracts'taki sorgu arayüzleriyle (ör. `IPlayerStatusQuery`).
  `tests/Architecture` (NetArchTest + `ModuleCatalog`) bunu her build'de denetler.
- **BuildingBlocks:** `Result/Error(ErrorKind)` → HTTP (`ResultHttpExtensions`), `Money`, `IClock`, `BusinessCalendar` (Asia/Famagusta; DB'ye UTC yazılır),
  `IPersonalDataProtector` (AES-GCM + HMAC blind index), `FourEyesDecision`, `IRequestContext` (kullanıcı, tenant, `X-Terminal`, `X-Correlation-Id`).
- **Değer kayıtları append-only:** DB tetikleyicileri UPDATE/DELETE'i engeller; finans ledger'ında denge kısıtı var. Düzeltme reversal/adjustment ile.
  Eşzamanlılık: PostgreSQL `xmin` + aggregate "touch" sayaçları → `ConcurrencyConflictException` → 409. Komutlar istemci kaynaklı idempotency anahtarı taşır.
- **Kimlik/yetki:** Keycloak OIDC JWT; `sub` ve `tenant_id` zorunlu. Ekran bazlı yetki `<ekran>:<işlem>`, endpoint'te `RequirePermission(...)`;
  ekran kataloğu Identity modülünde. Her sorgu tenant kapsamlı.
- **Sözleşme:** `docs/api/openapi-v1.json` kontrat testiyle kilitli; istemciler bu dosyadan tip üretir.
- Kararlar: `docs/adr/0001–0007`. İş tanımları: `specs/NNN-*/spec.md`. Terimler: `docs/sozluk.md`. Ekranlar: `docs/s-plus-ekranlar.md`.

## Kod kuralları (değiştirilemez; ayrıntı `docs/kod-kurallari.md`)

- **K1 SOLID/OOP:** tek sorumluluk; yeni davranış yeni sınıfla (strateji/adaptör); public setter yok, durum davranış metoduyla değişir;
  para/kimlik/kod value object (`Money`, `PlayerId`), para için `float/double` yok; tip kontrolü yerine polimorfizm.
- **K2 dikey dilim + sözleşme:** her işlev kendi modülü ve klasöründe; başka modülün iç koduna, DbContext'ine, tablosuna erişim yok.
- Değer kayıtları asla UPDATE/DELETE edilmez. Kritik işlem `CorrelationId`, `Actor`, `Terminal`, `ReasonCode` taşır ve audit'e yazılır.
- Cihaz/vendor protokolü (SAS/G2S, kapı kilidi, NEXIO) yalnızca entegrasyon adaptörlerinde; vendor→kanonik kod eşlemesi audit'li.
- Slot verisinin tek girişi `SlotMetersReportedV1` (NEXIO adaptörü, S Slot veya manuel). Opera/Elektra adaptörü yazılmaz.
- Rakip ürünlerin (NEXIO, Fidelio/OPERA, Elektra) ekranı/kodu/tasarımı kopyalanmaz; decompile yok.
- Gerçek oyuncu/misafir/kart verisi, üretim bağlantısı, secret repoya/loga/agent bağlamına girmez; yalnızca sentetik veri.
- Build analyzer'ları (BannedApiAnalyzers: `DateTime.Now` vb.) uyarıyı hata sayar; susturma gerekçeli olur.

## Çalışma akışı ve bitti tanımı

Spec (Spec Kit) → plan → önce başarısız test → kod → inceleme → demo. Bitti: kabul kriterleri testli; build uyarısız ve
tüm testler yeşil; `code-rules-reviewer` temiz; OpenAPI/Contracts güncel (kırıcı değişiklik = yeni sürüm); önemli karar ADR'de.
- Para/değer kodu (ledger, cage, chip, jackpot, comp, loyalty, folio) `finans-denetci`'den geçer, `finans` etiketi alır, insan (teknik lider) onayı olmadan birleşmez.
- Migration/şema/sorgu değişikliği `veri-sagligi-denetci`'den geçer.
- Paralel ajanlar ayrı worktree'de ve yalnızca kendi modül/klasöründe çalışır; birleştirmeyi koordinatör yapar.
- Güncel doküman: Context7 (.NET dahil). Kod haritası için `graphify` (varsa `graphify-out/`). Kurulu araçlar: `docs/araclar.md`.
