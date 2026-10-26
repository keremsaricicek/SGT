# Kod Kuralları

Projenin iki temel kod kuralı ve bunların her platformda nasıl uygulanacağı. `CLAUDE.md` bu kuralların özetini
içerir; bu doküman örnekleri, ihlal işaretlerini ve otomatik denetimi anlatır.

| Kural | Özet |
|---|---|
| **K1** | Kod SOLID ilkelerine göre yazılır; OOP kuralları esastır. |
| **K2** | Uygulamadaki her işlev ayrı kodlanır ve diğerleriyle tanımlı sözleşmeler üzerinden entegre olur. |

> Bu dokümandaki C# örnekleri kuralı anlatmak içindir. Kurulum ortamında derlenmedi; ilk kurulumda ad alanları
> ve `BuildingBlocks` tipleri projeye göre uyarlanmalıdır.

---

## 1. K1 — SOLID

Her ilke için: kural, bu projedeki karşılığı ve inceleyenin (insan veya agent) arayacağı ihlal işaretleri.

### 1.1 Single Responsibility (SRP)

**Kural:** Bir sınıfın tek bir değişme nedeni olur. "Tek metot" değil, "tek sorumluluk" demektir.

**Projede:** Check-in handler'ı akışı yönetir. Oda seçimi kuralı `IRoomAssignmentPolicy`'de, check-in kuralları
`Reservation` entity'sindedir. Kapı kartı basımı handler'da değil; Integration Hub `GuestCheckedIn` olayını
dinleyerek yapar.

```csharp
public sealed class CheckInHandler : ICommandHandler<CheckInCommand>
{
    private readonly IReservationRepository _reservations;
    private readonly IRoomAssignmentPolicy _roomPolicy;
    private readonly IClock _clock;

    public CheckInHandler(IReservationRepository reservations, IRoomAssignmentPolicy roomPolicy, IClock clock)
    {
        _reservations = reservations;
        _roomPolicy = roomPolicy;
        _clock = clock;
    }

    public async Task<Result> Handle(CheckInCommand command, CancellationToken ct)
    {
        var reservation = await _reservations.Get(command.ReservationId, ct);
        if (reservation is null) return HotelErrors.Reservation.NotFound;

        var room = _roomPolicy.Choose(reservation, command.RequestedRoom);
        var result = reservation.CheckIn(room, _clock.Now, command.Audit); // iş kuralı entity'de
        if (result.IsFailure) return result;

        await _reservations.Save(reservation, ct); // GuestCheckedIn olayı aynı transaction'da outbox'a yazılır
        return Result.Success();
    }
}
```

**İhlal işaretleri**
- Sınıf adında `Manager`, `Helper`, `Util`, `Service` ile biten ve her şeyi yapan sınıflar.
- Constructor'da 5'ten fazla bağımlılık.
- Aynı metotta doğrulama + kalıcılık + bildirim + biçimlendirme.
- Sınıfı açıklarken "ve" bağlacı gerekiyorsa ("rezervasyonu oluşturur **ve** e-posta atar **ve** raporu günceller").

### 1.2 Open/Closed (OCP)

**Kural:** Yeni davranış yeni sınıf ekleyerek gelir; çalışan kod değiştirilmez.

**Projede:** Kapı kilidi, POS, ödeme, slot protokolü gibi her vendor bir adaptör sınıfıdır. Comp uygunluk kuralları,
fiyat kuralları ve loyalty kuralları birer kural nesnesidir.

```csharp
public interface IDoorLockAdapter
{
    string VendorCode { get; }
    Task<KeyIssueResult> IssueKey(KeyRequest request, CancellationToken ct);
}

// Yeni vendor = yeni sınıf + DI kaydı. Mevcut kod değişmez.
public sealed class VendorADoorLockAdapter : IDoorLockAdapter
{
    public string VendorCode => "VENDOR_A";
    public Task<KeyIssueResult> IssueKey(KeyRequest request, CancellationToken ct) { /* ... */ }
}

public sealed class DoorLockAdapterResolver
{
    private readonly IReadOnlyDictionary<string, IDoorLockAdapter> _byVendor;

    public DoorLockAdapterResolver(IEnumerable<IDoorLockAdapter> adapters) =>
        _byVendor = adapters.ToDictionary(a => a.VendorCode);

    public IDoorLockAdapter For(string vendorCode) => _byVendor[vendorCode];
}
```

**İhlal işaretleri**
- İş kodunda `switch (vendorCode)`, `if (lockType == ...)`, `if (property.Country == ...)` zincirleri.
- Yeni bir tesis, vendor veya kural eklemek için çalışan bir handler'ın değiştirilmesi.

### 1.3 Liskov Substitution (LSP)

**Kural:** Bir arayüzü uygulayan her sınıf, o arayüzün beklendiği her yerde sorunsuz çalışır.

```csharp
// YASAK: sözleşmeyi bozan uygulama
public sealed class ArchiveFolioRepository : IFolioRepository
{
    public Task Save(Folio folio, CancellationToken ct) => throw new NotSupportedException();
}
// Doğrusu: arayüzü ayırmak (bkz. ISP). Arşiv yalnızca IFolioReader'ı uygular.
```

**İhlal işaretleri**
- `NotSupportedException` / `NotImplementedException` fırlatan arayüz metotları.
- Çağıran kodda `if (x is CorporateRate)` gibi somut tip kontrolleri.
- Alt sınıfın üst sınıftan daha sıkı ön koşul veya daha gevşek sonuç garantisi vermesi
  (ör. bir `IRateCalculator` uygulamasının negatif tutar dönebilmesi).

### 1.4 Interface Segregation (ISP)

**Kural:** Arayüzler küçük ve kullanıcısına özeldir.

```csharp
public interface IReservationReader
{
    Task<ReservationView?> Get(ReservationId id, CancellationToken ct);
    Task<IReadOnlyList<ArrivalView>> Arrivals(PropertyId property, DateOnly date, CancellationToken ct);
}

public interface IReservationWriter
{
    Task Save(Reservation reservation, CancellationToken ct);
}
// Mobil arrival listesi yalnızca IReservationReader'a bağlıdır.
```

**İhlal işaretleri**
- 7'den fazla metotlu arayüzler.
- Uygulamaların bazı metotları boş bırakması veya exception fırlatması.

### 1.5 Dependency Inversion (DIP)

**Kural:** Domain ve Application altyapıyı tanımaz; soyutlamalara bağlıdır. Somut altyapı `Infrastructure`
projesinde yazılır ve DI ile verilir.

| Domain/Application şunu kullanır | Şunu kullanmaz |
|---|---|
| `IClock` | `DateTime.Now`, `DateTime.UtcNow` |
| `IEventPublisher` / outbox | Kafka/RabbitMQ istemcisi |
| `IFolioRepository` | `DbContext`, SQL |
| `IPlayerTierQuery` (Contracts) | `HttpClient`, başka modülün sınıfları |
| `ILogger<T>` | `Console` |

**İhlal işaretleri**
- Domain veya Application içinde altyapı sınıfının `new` ile oluşturulması.
- Statik servis erişimi (`ServiceLocator.Get<T>()`), statik değişken durum.
- `DateTime.Now` ve benzerleri derleme hatası verir (`build/BannedSymbols.txt`).

---

## 2. K1 — OOP kuralları

1. **Kapsülleme.** Entity'lerde public setter yoktur. Durum, kuralları uygulayan davranış metotlarıyla değişir.
   Nesne her zaman geçerli durumda oluşturulur (constructor veya factory metotta doğrulama).
2. **Zengin domain modeli.** İş kuralı entity ve value object'tedir; handler'lar yalnızca akışı yönetir.
   Yalnızca property taşıyan entity + kuralları içeren "servis" (anemik model) yazılmaz.
3. **Value object.** Para, kimlik ve kodlar primitive değildir: `Money`, `PlayerId`, `RoomNumber`, `ReasonCode`.
   Value object'ler immutable'dır ve değerleriyle karşılaştırılır.
4. **Para.** `decimal` + para birimi; `float`/`double` yasaktır. Farklı para birimleri örtük toplanmaz.
5. **Immutability.** Olaylar, DTO'lar ve komutlar `record` ve immutable'dır.
6. **Kompozisyon.** Kalıtım yalnızca gerçek "is-a" ilişkisinde; davranış paylaşımı için kompozisyon.
7. **Polimorfizm.** Tip kontrolü yerine sanal metot veya strateji.
8. **Hata modeli.** Beklenen iş kuralı ihlalleri `Result`/`Error` ile döner; exception beklenmeyen durumlar içindir.
   (Varsayılan karar; değiştirilecekse ADR ile.)

Örnek: kapsülleme + değer hareketi bütünlüğü bir arada.

```csharp
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency) { Amount = amount; Currency = currency; }

    public static Money Of(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("ISO 4217 para birimi gerekli.", nameof(currency));
        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency) => Of(0m, currency);

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException($"Para birimi uyuşmuyor: {Currency} / {other.Currency}");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Negate() => new(-Amount, Currency);
}

public sealed class Folio
{
    private readonly List<FolioPosting> _postings = new();

    public FolioId Id { get; }
    public string Currency { get; }
    public bool IsClosed { get; private set; }
    public IReadOnlyList<FolioPosting> Postings => _postings;
    public Money Balance => _postings.Aggregate(Money.Zero(Currency), (sum, p) => sum.Add(p.Amount));

    // constructor, Close() vb. kısaltıldı

    public Result<FolioPosting> Post(Money amount, PostingCode code, AuditContext audit)
    {
        if (IsClosed) return HotelErrors.Folio.Closed;
        if (amount.Currency != Currency) return HotelErrors.Folio.CurrencyMismatch;

        var posting = FolioPosting.Create(Id, amount, code, audit);
        _postings.Add(posting);
        return posting;
    }

    // Düzeltme: mevcut kayıt değiştirilmez, ters kayıt eklenir.
    public Result<FolioPosting> Reverse(FolioPostingId postingId, ReasonCode reason, AuditContext audit)
    {
        var original = _postings.SingleOrDefault(p => p.Id == postingId);
        if (original is null) return HotelErrors.Folio.PostingNotFound;
        if (_postings.Any(p => p.ReversesId == postingId)) return HotelErrors.Folio.AlreadyReversed;

        var reversal = FolioPosting.ReversalOf(original, reason, audit);
        _postings.Add(reversal);
        return reversal;
    }
}
```

### Ölçülülük kuralı (aşırı mühendisliği önlemek için)

SOLID "her şeye arayüz" demek değildir. Agent'ların en sık hatası gereksiz katman ve arayüz üretmektir.

| Arayüz **gerekir** | Arayüz **gerekmez** |
|---|---|
| Altyapı sınırları (repository, mesajlaşma, saat, dosya, dış servis) | Value object, DTO, komut, olay |
| Modüller arası sözleşmeler (`*.Contracts`) | Domain entity |
| Birden fazla uygulaması olan davranışlar (vendor adaptörü, kural, strateji) | Saf hesaplama sınıfları |
| Testte değiştirilmesi gereken bağımlılıklar | Tek uygulamalı, test sınırı olmayan iç yardımcılar |

---

## 3. K2 — Her işlev ayrı kodlanır, sözleşmelerle entegre olur

### 3.1 Terimler

- **Modül:** Bir bounded context. Identity, Party, Hotel, CasinoPlayer, GamingFloor, Finance, Loyalty,
  Workflow, Integration, Audit.
- **İşlev:** Kullanıcının veya sistemin yaptığı tek bir iş: rezervasyon oluşturma, check-in, comp onayı,
  cashier vardiya kapanışı, puan kullanımı.

Ürünlerle ilişki (taslak; Faz 0'da kesinleşir): **S Plus** = Identity, Party, CasinoPlayer, GamingFloor (canlı oyun +
slot ekranları), Finance (kasa), Loyalty, Workflow, Audit · **S Slot** = slot veri toplama modülü · **S Vista** = Hotel ·
Integration = NEXIO adaptörü. Slot ekranları veriyi tek bir Contracts sözleşmesinden (ör. `ISlotDataSource`) alır;
arkasında NEXIO adaptörü veya S Slot vardır.

"Her işlev ayrı kodlanır" her C# metodunun ayrı dosyada olması anlamına gelmez; bu OOP'deki bütünlüğü bozar.
Anlamı: her işlevin kodu kendi klasöründe, diğer işlevlerin koduna dokunmadan yazılır ve test edilir.

### 3.2 Klasör ve proje yapısı

```text
src/
  Bootstrapper/Platform.Api/              tek host; modülleri kaydeder, kendi iş kodu yoktur
  BuildingBlocks/                         ortak soyutlamalar: Result, Error, Money, IClock, IEventPublisher, Outbox/Inbox
  Modules/
    Hotel/
      Platform.Modules.Hotel.Contracts/   dışarıya açık TEK yüzey: sorgu arayüzleri, DTO'lar, integration event'ler
      Platform.Modules.Hotel.Domain/      entity, value object, domain event, policy (altyapı bağımlılığı yok)
      Platform.Modules.Hotel.Application/
        Reservations/
          CreateReservation/
            CreateReservationCommand.cs
            CreateReservationHandler.cs
            CreateReservationValidator.cs
          GetArrivals/
        FrontDesk/
          CheckIn/
        Folio/
          PostCharge/
          ReversePosting/
        IntegrationEvents/                diğer modüllerden gelen olayların handler'ları
      Platform.Modules.Hotel.Infrastructure/  EF Core, repository, outbox/inbox, dış istemciler
      Platform.Modules.Hotel.Api/         endpoint'ler: yalnızca HTTP ↔ komut/sorgu çevirisi
    CasinoPlayer/  ...aynı yapı
    Finance/       ...aynı yapı
tests/
  Architecture/                           K1/K2 otomatik denetimi
  Modules/Hotel/                          unit + integration testler
  Contracts/                              modüller arası kontrat testleri
```

Ad alanı kuralı: `Platform.Modules.{Modül}.{Katman}`. Mimari testler bu kurala dayanır.
(`Platform` ürün adı belirlenene kadar yer tutucudur.)

### 3.3 Bağımlılık kuralları

| Proje | Bağımlı olabilir | Bağımlı olamaz |
|---|---|---|
| Domain | BuildingBlocks | Application, Infrastructure, Api, EF Core, ASP.NET, mesajlaşma |
| Application | Domain, kendi Contracts'ı, **diğer modüllerin Contracts'ı**, BuildingBlocks | Infrastructure, Api, EF Core, mesajlaşma istemcileri |
| Infrastructure | Application, Domain, BuildingBlocks, altyapı kütüphaneleri | Diğer modüllerin Domain/Application/Infrastructure'ı |
| Api | Application, Contracts | Infrastructure (yalnızca DI kaydı hariç), diğer modüllerin iç projeleri |
| Contracts | BuildingBlocks | Her modülün iç projeleri, framework'ler |

### 3.4 Entegrasyon yolları

**A. Olay (varsayılan; durum değişikliği için).** Örnek: fizibilite raporundaki "room comp" senaryosu.
Casino tarafında comp onaylanır → `CompApprovedV1` olayı outbox'a yazılır → Hotel modülü olayı dinler ve
folio'ya allowance uygular → Audit modülü kaydeder.

```csharp
// Platform.Modules.CasinoPlayer.Contracts
public sealed record CompApprovedV1(
    Guid EventId,          // idempotency anahtarı
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    Guid PropertyId,
    Guid CompId,
    Guid PlayerId,
    Guid ReservationId,
    decimal Amount,
    string Currency,
    string ApprovedBy,
    string ReasonCode) : IIntegrationEvent;

// Platform.Modules.Hotel.Application/IntegrationEvents
public sealed class CompApprovedHandler : IIntegrationEventHandler<CompApprovedV1>
{
    private readonly IFolioRepository _folios;
    private readonly IInbox _inbox;
    private readonly IUnitOfWork _unitOfWork;

    public CompApprovedHandler(IFolioRepository folios, IInbox inbox, IUnitOfWork unitOfWork)
    {
        _folios = folios;
        _inbox = inbox;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompApprovedV1 e, CancellationToken ct)
    {
        if (await _inbox.AlreadyProcessed(e.EventId, ct)) return; // aynı olay ikinci hareket üretmez

        var folio = await _folios.GetByReservation(new ReservationId(e.ReservationId), ct);
        var result = folio.ApplyAllowance(Money.Of(e.Amount, e.Currency), AllowanceSource.CasinoComp(e.CompId),
                                          AuditContext.From(e));
        if (result.IsFailure)
            throw new IntegrationHandlingException(e.EventId, result.Error); // retry → DLQ → operatör ekranı

        _inbox.MarkProcessed(e.EventId);
        await _unitOfWork.Commit(ct); // folio değişikliği ve inbox kaydı tek transaction'da
    }
}
```

Integration event kuralları:
- Adı geçmiş zamandadır ve sürüm taşır: `GuestCheckedInV1`, `CompApprovedV1`.
- Immutable `record`'dur; tam nesne değil kimlik ve gerekli alanları taşır.
- `EventId`, `CorrelationId`, `OccurredAt`, `PropertyId` zorunludur.
- Yayıncı olayı outbox ile, iş verisiyle aynı transaction'da yazar.
- Tüketici idempotent'tir (inbox) ve işlem + inbox kaydı aynı transaction'dadır.
- Kırıcı değişiklik yeni sürümle (`V2`) yapılır; eski sürüm deprecation süresince yayınlanır.

**B. Senkron sorgu (yalnızca okuma).** Örnek: rezervasyon ekranında oyuncu tier'ı göstermek.

```csharp
// Platform.Modules.CasinoPlayer.Contracts
public interface IPlayerTierQuery
{
    Task<PlayerTierDto?> GetTier(Guid playerId, CancellationToken ct);
}
// Uygulaması CasinoPlayer modülündedir; Hotel yalnızca arayüzü kullanır.
```

**C. Yasak yollar**
- Bir modülün başka modülün tablolarına yazması veya okuması, modüller arası SQL join.
- Başka modülün Domain/Application sınıflarını referans almak; entity sınıflarını paylaşmak.
- Durum değiştiren senkron modüller arası çağrı (ör. Hotel'in Finance'te doğrudan hareket oluşturması).

### 3.5 Bir işlevin eksiksiz olması

Her işlev şu parçalarla tamamlanır:

1. Komut veya sorgu (`record`)
2. Handler
3. Validator
4. Endpoint (gerekiyorsa)
5. Unit testler (domain kuralları) ve integration testler (handler + veritabanı)
6. Public sözleşme değiştiyse OpenAPI güncellemesi ve kontrat testi
7. Olay yayınlıyor veya tüketiyorsa olay şeması ve tüketici idempotency testi

---

## 4. Platforma göre uygulama

| Platform | K1 (SOLID/OOP) | K2 (işlev ayrımı) |
|---|---|---|
| **Backend (.NET)** | Tam uygulanır; bu dokümandaki örnekler. | Modül + dikey dilim yapısı (bölüm 3). |
| **Windows EXE (WPF)** | MVVM. View'de (code-behind) iş mantığı yok. ViewModel bağımlılıkları arayüzle ve DI ile alır. API istemcisi arayüz arkasındadır. | Her ekran işlevi kendi feature klasöründe (`Features/FrontDesk/CheckIn/`: View, ViewModel, servis arayüzü, testler). Navigasyon servis üzerinden. |
| **Mobil (Flutter)** | Dart OOP: repository arayüzleri domain'de, uygulamaları data katmanında. Widget'lar iş kuralı içermez. State yönetimi seçimi (Riverpod/Bloc) Discovery'de ADR ile. | Feature-first klasörleme: `lib/features/arrivals/{data,domain,presentation}`. Feature'lar birbirinin içine değil ortak `core/` sözleşmelerine bağlıdır. |
| **Web (React/Next.js)** | React'te sınıf bileşeni kullanılmaz; SOLID bileşen, hook ve servis düzeyinde uygulanır: tek işli bileşen (SRP), props/kompozisyonla genişletme (OCP), API erişimi `services/` arayüzleri üzerinden (DIP). Sınıf tabanlı OOP yalnızca domain/servis katmanında. | `src/features/{reservations,players,...}`; feature'lar birbirinin iç bileşenlerini import etmez, paylaşılanlar `src/shared/`'dadır. |

Hiçbir istemci veritabanına bağlanmaz; tüm istemciler aynı API sözleşmelerini kullanır.

---

## 5. İnceleme eşikleri

Aşağıdaki eşikler kesin yasak değil, uyarıdır. Aşılırsa PR açıklamasında gerekçe yazılır.

| Ölçü | Eşik |
|---|---|
| Sınıf uzunluğu | 300 satır |
| Metot uzunluğu | 40 satır |
| Constructor bağımlılığı | 5 |
| Arayüz metodu sayısı | 7 |
| İç içe `if`/döngü derinliği | 3 |
| Cyclomatic complexity (metot) | 10 |

SonarQube kalite kapısı bu eşiklere göre ayarlanır.

---

## 6. Otomatik denetim

Kurallar agent'ların iyi niyetine bırakılmaz; dört katmanda denetlenir.

| Katman | Dosya | Ne yakalar |
|---|---|---|
| Derleme | `Directory.Build.props`, `build/BannedSymbols.txt` | Nullable, uyarı = hata, analyzer'lar; Domain/Application'da `DateTime.Now`, `Console`, `HttpClient`, `Thread.Sleep` |
| Mimari test | `tests/Architecture/ArchitectureTests.cs` | Modüller arası iç referans, Domain/Application'ın altyapıya bağımlılığı, Contracts saflığı, Domain'de public setter |
| Agent incelemesi | `.claude/agents/code-rules-reviewer.md` | SRP/OCP/LSP/ISP ihlalleri, anemik model, entegrasyon ve değer hareketi kuralları |
| Kalite kapısı | SonarQube (bölüm 5 eşikleri) | Karmaşıklık, tekrar, kapsam |

Yeni bir modül eklendiğinde `tests/Architecture/ArchitectureTests.cs` içindeki `ModuleCatalog`'a bir satır eklenir.
Katalogda olmayan modül denetlenmez.
