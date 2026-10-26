# CLAUDE.md — SGT (Sentez Gaming Technologies)

Bu dosya her oturumda okunur; kısa tutulur, ayrıntılar `docs/` altındadır.
Çelişki olursa öncelik sırası: `.specify/memory/constitution.md` → `docs/kararlar.md` → bu dosya →
diğer `docs/` dosyaları → `docs/fizibilite-raporu.pdf` (en eski kaynak; kararlarla çelişirse geçersiz).

**Durum: Faz 1 (ortak çekirdek). İskelet kuruldu; her iş spec → test → kod sırasıyla.** Proje sahibi kod bilmez; açıklamalar sade Türkçe olmalı.

## Proje

KKTC casino ve otelleri için ayrı satılabilen ürün ailesi (ayrıntı: `docs/kararlar.md`):
- **S Plus:** ana casino sistemi; oyuncular, kasa, loyalty, canlı oyun, slot ekranları, raporlar, yetki paneli.
- **S Slot:** slot makinelerinden veri toplar (NEXIO'nun yerine geçer); isteğe bağlı modül.
- **S Vista:** otel sistemi; isteğe bağlı modül.

Kesin kurallar:
- Rakip sistemlerden tek dış bağlantı NEXIO'dur (isteğe bağlı). Slot verisi tek girişten gelir:
  NEXIO adaptörü veya S Slot. Opera/Elektra adaptörü yazılmaz.
- Modüller lisansa göre açılır/kapanır; S Plus tek başına çalışır.
- Yetki ekran bazındadır: her ekran için "görebilir" ve "işlem yapabilir" ayrı verilir, yetki panelinden yönetilir,
  her değişiklik audit'e yazılır.
- Para birimleri TRY/EUR/GBP/USD; arayüz TR/EN.

İstemciler: Windows uygulaması (operasyon), iOS/Android, web paneli. Mimari: API-first, event-driven, modüler monolit.

Stack (Discovery sonunda ADR ile kesinleşir): .NET 10 / ASP.NET Core · PostgreSQL · WPF (MVVM) ·
Flutter · React/Next.js · Kafka veya RabbitMQ · OIDC (Keycloak/Entra sınıfı) · OpenTelemetry.

## Kod kuralları (değiştirilemez)

**K1 — SOLID ve OOP esastır.**
- Her sınıfın tek bir değişme nedeni vardır (SRP).
- Yeni davranış, mevcut kodu değiştirerek değil yeni sınıf ekleyerek gelir: strateji, adaptör, kural nesnesi (OCP).
- Alt tip, üst tipin sözleşmesini bozmaz; arayüz metodu `NotSupportedException` ile "boş geçilmez" (LSP).
- Arayüzler küçük ve role özeldir; hiçbir sınıf kullanmadığı metoda bağımlı olmaz (ISP).
- Domain ve Application soyutlamalara bağlıdır; altyapı (EF Core, mesajlaşma, HTTP, saat, cihaz protokolü) dışarıdan enjekte edilir (DIP).
- Kapsülleme: durum yalnızca davranış metotlarıyla değişir, public setter yoktur; iş kuralı entity ve value object içindedir.
- Para, kimlik ve kod gibi kavramlar primitive değil value object'tir (`Money`, `PlayerId`, `RoomNumber`). Para için `float`/`double` kullanılmaz.
- Kalıtım yerine kompozisyon; tip kontrolü (`is`, `switch` on type) yerine polimorfizm.

**K2 — Her işlev ayrı kodlanır, sözleşmelerle entegre olur.**
- Her işlev (rezervasyon oluşturma, check-in, comp onayı, cashier kapanışı...) kendi modülünde, kendi klasöründe
  dikey dilim olarak yazılır: komut/sorgu + handler + validator + endpoint + testler.
- Modüller birbirinin iç koduna, DbContext'ine ve tablolarına erişmez. Tek temas noktaları:
  modülün `*.Contracts` projesindeki arayüz/DTO'lar ve integration event'ler.
- Durum değiştiren modüller arası iletişim: integration event + outbox. Anlık okuma: Contracts sorgu arayüzü.
- Her modül tek başına derlenir ve test edilir; entegrasyon kontrat testleriyle kanıtlanır.
- `tests/Architecture` bu kuralları her build'de denetler. Kırmızıysa iş bitmemiştir.

Ayrıntılar, örnekler ve platform bazlı uygulama: `docs/kod-kurallari.md`

## Mimari kurallar (fizibilite raporundan)

- İstemciler veritabanına asla doğrudan bağlanmaz; her işlem kimliği doğrulanmış API veya olay üzerinden geçer.
- Finansal ve değer taşıyan kayıtlar asla UPDATE/DELETE edilmez; düzeltme reversal veya adjustment kaydıyla yapılır.
- Her kritik işlem `CorrelationId`, `Actor`, `Terminal` ve `ReasonCode` taşır ve audit'e yazılır.
- Her entegrasyon mesajı idempotency anahtarı taşır; aynı mesaj iki kez gelirse ikinci hareket oluşmaz.
- Core domain cihaz/vendor protokollerini (TCP, serial, FIAS...) bilmez; bunlar Integration Hub adaptörlerindedir.
- Vendor kodu → kanonik kod dönüşümü mapping katmanındadır; mapping değişikliği audit edilir.
- Retry: exponential backoff + dead-letter queue + operatör reprocess ekranı.
- Her sorgu ve komut tenant/property kapsamındadır; kapsamsız veri erişimi yazılmaz.
- Offline/edge modda finansal ve gaming değer hareketi yalnızca risk modelinde tanımlı limitlerle açılır.
- Public/partner API ile internal API ayrıdır; API'ler OpenAPI contract-first ve sürümlüdür.
- Rakip ürünlerin (NEXIO, Fidelio/Suite8/OPERA, Elektra) ekranı, kaynak kodu veya ticari tasarımı kopyalanmaz;
  rakip yazılımların kodu açılmaz (decompile/REA yok). Özellik fikri yalnızca kullanım, demo ve kılavuzdan öğrenilir.

## Güvenlik ve veri

- Gerçek misafir/oyuncu verisi, kart verisi, üretim bağlantı bilgisi ve secret asla repoya, koda, loga veya
  agent bağlamına girmez. Yalnızca sentetik test verisi kullanılır.
- Secret'lar secret manager'dadır; istemci uygulamalarda secret veya DB bilgisi bulunmaz.
- Hassas alanlar loglanmaz; gerekiyorsa maskelenir.
- Web/API için OWASP ASVS, mobil için OWASP MASVS esas alınır.

## Çalışma şekli

- Önce spec ve plan (Spec Kit), sonra test, sonra kod. Plansız kod yazılmaz.
- Testi olmayan davranış yoktur: önce başarısız test, sonra kod.
- Her agent yalnızca kendisine atanan modül ve klasörlerde dosya değiştirir.
- Para/değer akışlarına dokunan PR'lar (ledger, folio, wallet, cage, comp, jackpot) `finans` etiketi alır
  ve insan onayı olmadan birleşmez.
- Belirsizlikte tahmin edilmez; soru yazılır ve durulur.
- UI işi ekran görüntüsüyle doğrulanmadan bitmiş sayılmaz (web: Playwright MCP).
- Kütüphane/API kullanımında güncel doküman: .NET için Microsoft Learn MCP, diğerleri için Context7.

## Bitti tanımı

1. Spec'teki kabul kriterleri testlerle karşılanıyor.
2. Build uyarısız; unit, integration, contract ve architecture testleri yeşil.
3. `code-rules-reviewer` agent'ı ve güvenlik taraması temiz, ya da her bulgu gerekçeli.
4. Public sözleşme değiştiyse OpenAPI ve Contracts güncel; kırıcı değişiklik yeni sürümle yapılmış.
5. Önemli bir karar alındıysa `docs/adr/` altında ADR yazılmış.

## Komutlar

```bash
dotnet build Sgt.slnx                                  # derleme (uyarı = hata)
dotnet test Sgt.slnx                                   # tüm testler
dotnet test tests/Architecture/Sgt.Tests.Architecture.csproj   # yalnızca mimari kurallar
dotnet run --project src/Bootstrapper/Sgt.Api          # lokal API; /health
```
Migration (modül başına): `dotnet ef migrations add <Ad> -p src/Modules/<Modül>/Sgt.Modules.<Modül>.Infrastructure -o Persistence/Migrations`
Entegrasyon testleri yerel PostgreSQL ister (`SGT_TEST_DB` ya da `Host=localhost;Username=sgt;Password=sgt_dev_only`).
