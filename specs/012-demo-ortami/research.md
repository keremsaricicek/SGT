# Araştırma: Demo ortamı

## R1 Veri nasıl yazılır?
- Karar: Host içinde modül `ICommandHandler`'ları DI'dan çağrılır.
- Gerekçe: K2'ye uyar (tablo erişimi yok), iş kuralları ve audit aynen çalışır; HTTP'ye göre hızlı.
- Alternatif: HTTP API üzerinden istemci → geçmiş tarih verilemez, yavaş. SQL dump → K2 ve append-only tetikleyicileri ihlal eder.

## R2 30 günlük geçmiş
- Karar: `DemoClock : IClock`, yalnız `--seed-demo` modunda singleton olarak SystemClock yerine kaydedilir.
- Gerekçe: Handler'lar zamanı yalnız IClock'tan alır (BannedApiAnalyzers bunu garanti eder).

## R3 Olay akışı (rating → puan)
- Karar: `IOutboxDrainer` (modül başına `OutboxProcessor<T>` sarmalayıcı); her simüle gün sonunda hepsi 0 dönene kadar `ProcessBatch`.
- Gerekçe: Hosted service'e ve gerçek zamana bağlı kalmadan deterministik; olay zamanı demo saatiyle tutarlı.

## R4 İdempotency
- Karar: Demo tenant'ında oyuncu varsa (`IPlayerSearch` / mevcut Contracts sorgusu) üretici hiçbir şey yazmaz; `reset` DB volume'unu siler.

## R5 Demo kullanıcıları
- Karar: Keycloak realm JSON'a sabit UUID'li 5 kullanıcı eklenir; şifreler `${SGT_DEMO_PASSWORD}` ile env'den. Identity tarafında
  `RegisterUser` + rol (`CreateDefaultRoles` + seeder'ın oluşturduğu "Kasiyer" ve "Host" rolleri) + `AssignRole`.

## R6 Docker
- Bulut oturumunda Docker yok: `demo.sh` burada çalıştırılamaz; seeder entegrasyon testi yerel PostgreSQL ile doğrulanır,
  compose yolu kullanıcı makinesinde/CI'da doğrulanır.
