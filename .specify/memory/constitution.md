# SGT (Sentez Gaming Technologies) — Proje Anayasası

Ürün ailesi: S Plus (ana casino sistemi), S Slot (slot veri toplama), S Vista (otel). Ürün kararları: `docs/kararlar.md`.

Bu anayasa, platformda yazılan her satır kod için geçerli olan değiştirilemez ilkeleri tanımlar.
Her spec, plan ve görev bu ilkelere uymak zorundadır. Diğer tüm kural dosyalarından önce gelir.

## Temel İlkeler

### I. SOLID ve Nesne Yönelimli Tasarım

Tüm kod SOLID ilkelerine ve OOP kurallarına göre yazılır.

- Her sınıfın tek bir değişme nedeni olmalıdır (Single Responsibility).
- Yeni davranış mevcut sınıfları değiştirerek değil, yeni sınıflar ekleyerek eklenmelidir (Open/Closed).
  Vendor, kural ve hesaplama varyasyonları strateji veya adaptör nesneleriyle çözülür.
- Bir arayüzü uygulayan her sınıf o arayüzün sözleşmesini eksiksiz yerine getirmelidir (Liskov Substitution).
- Arayüzler küçük ve role özel olmalıdır (Interface Segregation).
- Domain ve uygulama katmanları soyutlamalara bağlı olmalıdır; altyapı dışarıdan enjekte edilir (Dependency Inversion).
- Durum yalnızca davranış metotlarıyla değişir; iş kuralları entity ve value object'lerde kapsüllenir.
- Para, kimlik ve kod gibi alan kavramları value object ile temsil edilir.

Gerekçe: Platform yıllarca büyüyecek, çok sayıda vendor entegrasyonu ve tesis kuralı eklenecek ve kodun büyük
kısmı paralel çalışan agent'lar tarafından üretilecek. Bu ilkeler, bir değişikliğin etkisini tek bir yere
sınırlayarak hem insan hem agent incelemesini mümkün kılar.

### II. İşlev Bazlı Modülerlik ve Sözleşmeli Entegrasyon

Uygulamadaki her işlev ayrı kodlanır ve diğer işlevlerle yalnızca tanımlı sözleşmeler üzerinden entegre olur.

- Platform, bounded context'lere karşılık gelen modüllerden oluşur: Identity, Party, Hotel, CasinoPlayer,
  GamingFloor, Finance, Loyalty, Workflow, Integration, Audit.
- Her işlev kendi modülünde, kendi klasöründe dikey dilim olarak yazılır.
- Bir modül başka bir modülün iç koduna, veritabanı tablolarına veya DbContext'ine erişemez.
- Modüller arası tek temas yüzeyi `*.Contracts` projesidir: sorgu arayüzleri, DTO'lar ve integration event'ler.
- Durum değiştiren modüller arası iletişim integration event ve outbox ile yapılır; senkron çağrı yalnızca okumadır.
- Her modül bağımsız derlenir ve test edilir; modüller arası uyum kontrat testleriyle kanıtlanır.
- Bu kurallar mimari testlerle otomatik denetlenir; mimari testi kırmızı olan değişiklik birleştirilmez.

Gerekçe: Paralel agent'lar ancak dosya ve sorumluluk sınırları net olduğunda çakışmadan çalışabilir. Ayrıca
ileride Casino Ingestion, Payments, Notifications ve Reporting gibi parçaların servise ayrılabilmesi bu
sınırlara bağlıdır.

### III. Değer Hareketlerinin Bütünlüğü (PAZARLIK KONUSU DEĞİL)

- Finansal ve değer taşıyan kayıtlar (ledger, folio posting, wallet, cage, voucher/ticket, comp, jackpot, puan)
  append-only'dir; UPDATE ve DELETE yasaktır.
- Düzeltme yalnızca reversal veya adjustment kaydıyla yapılır ve gerekçe kodu taşır.
- Her entegrasyon mesajı ve her değer hareketi idempotency anahtarı taşır; tekrar gelen mesaj ikinci hareket üretmez.
- Her kritik para/değer hareketi audit edilebilir, tekrar oynatılabilir ve reconciliation raporuna girebilir olmalıdır.
- Yüksek riskli işlemler maker-checker (dört göz) onayından geçer.
- Para tutarları ondalık tipte (`decimal`) ve para birimiyle birlikte tutulur.

### IV. API-Only Erişim ve Tenant İzolasyonu

- İstemciler (Windows, mobil, web) veritabanına asla doğrudan bağlanmaz.
- Her işlem kimliği doğrulanmış ve yetkilendirilmiş API veya olay üzerinden geçer.
- Her veri erişimi tenant/property kapsamındadır; izolasyon otomatik testlerle kanıtlanır.
- API'ler OpenAPI contract-first tasarlanır, sürümlenir; public/partner API internal API'den ayrıdır.

### V. Test-Önce Geliştirme ve Doğrulanabilirlik

- Davranış kodu, onu doğrulayan başarısız test yazıldıktan sonra yazılır.
- CI zinciri: unit + integration + contract + architecture + migration + security + performans smoke testleri.
- Her entegrasyon için simulator, kontrat testi ve hata/retry test seti bulunur.
- Night audit, cashier kapanışı, comp onayı ve cihaz olayı gibi kritik akışlar için adversarial test verileri bulunur.

### VI. Varsayılan Güvenlik ve Veri Minimizasyonu

- Gerçek misafir/oyuncu verisi, kart verisi ve secret'lar geliştirme ortamına, koda, loga veya agent bağlamına girmez.
- Least privilege, MFA, ayrık yetkili hesaplar; hassas alanlarda alan düzeyinde şifreleme.
- Web/API OWASP ASVS'e, mobil OWASP MASVS'e göre doğrulanır; SAST, SCA, secrets taraması CI'dadır.
- İstemci uygulama paketleri secret veya veritabanı bilgisi içermez.

### VII. İzlenebilirlik

- Her kritik işlem `CorrelationId`, `Actor`, `Terminal`, `ReasonCode` ile audit'e yazılır; audit append-only'dir.
- Her işlem log, trace ve metrik üzerinde correlation ID ile izlenebilir (OpenTelemetry).

### VIII. Modüler Ürün Paketleme ve Ekran Bazlı Yetki

- S Plus tek başına çalışır; S Slot ve S Vista lisansa göre açılan modüllerdir. Bir modülün yokluğu diğerlerini bozmaz.
- Slot verisi tek bir giriş sözleşmesinden gelir; arkasında NEXIO adaptörü veya S Slot bulunur. Ekranlar kaynağı bilmez.
- Rakip sistemlerden tek dış bağlantı NEXIO'dur.
- Her ekran için "görebilir" ve "işlem yapabilir" yetkileri ayrı tanımlanır, panelden verilir ve audit edilir.
  Yetkisiz ekran kullanıcıya hiç gösterilmez; yetki kontrolü her zaman sunucuda yapılır.

## Ek Kısıtlar

- **Pazar:** KKTC. TRY/EUR/GBP/USD çoklu para birimi ve TR/EN arayüz zorunludur.

- **Mimari evrim:** Başlangıç modüler monolittir. Bir modülün servise ayrılması yalnızca ölçülmüş bir ihtiyaçla
  ve ADR ile yapılır.
- **Entegrasyon:** Core domain cihaz/vendor protokollerini bilmez. Protokoller Integration Hub adaptörlerindedir;
  vendor → kanonik kod dönüşümü audit edilen mapping katmanındadır; retry + DLQ + operatör reprocess zorunludur.
- **Offline/Edge:** Offline modda hangi işlemlerin açık olduğu, limitleri ve sonradan reconciliation davranışı
  ayrı risk modeliyle tanımlanır; tanımsız işlem offline yapılamaz.
- **Özgünlük:** Rakip ürünlerin ekranları, kaynak kodu veya ticari tasarımı kopyalanmaz.
- **Stack:** .NET tabanlı backend, PostgreSQL, WPF/WinUI Windows istemcisi, Flutter mobil, React/Next.js web,
  Kafka/RabbitMQ olay altyapısı. Kesin seçim Discovery sonunda ADR ile dondurulur.

## Geliştirme Akışı

1. `/speckit.specify` ile spec yazılır; kabul kriterleri ölçülebilir olmalıdır.
2. `/speckit.plan` ile plan çıkarılır. Planın "Constitution Check" bölümü bu ilkelerin her birine uyumu gösterir.
3. `/speckit.tasks` ile görevler; her görev tek modüle ve tek işleve ait olmalıdır.
4. Test → uygulama → `code-rules-reviewer` → güvenlik taraması → PR.
5. Değer hareketine dokunan PR'lar `finans` etiketi alır ve insan uzman onayı olmadan birleşmez.

## Yönetişim

- Bu anayasa diğer tüm kural ve alışkanlıklardan önce gelir.
- İlkelerden sapma yalnızca ADR ile gerekçelendirilir ve planın "Complexity Tracking" tablosuna yazılır.
- Değişiklik: ADR + proje sahibi onayı + sürüm artışı. MAJOR: ilke kaldırma veya anlamını değiştirme;
  MINOR: yeni ilke veya bölüm; PATCH: açıklama ve ifade düzeltmesi.
- Her PR incelemesi bu anayasaya uyumu kontrol eder.

**Sürüm**: 1.1.0 | **Durum**: Taslak, proje sahibi onayı bekliyor | **Oluşturulma**: 2026-10-26
