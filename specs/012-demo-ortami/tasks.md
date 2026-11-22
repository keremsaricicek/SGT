# Görevler: Demo ortamı (Spec 012)

Girdi: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [contracts/demo-cli.md](contracts/demo-cli.md).
Test-önce (anayasa V): her hikâyede test önce yazılır ve başarısız görülür.

Hikâyeler: **US1** demo verisi üretimi (KK2–KK5, KK7, KK8) · **US2** rol bazlı demo kullanıcıları (KK6) · **US3** tek komut (KK1).

## Faz 1: Kurulum
- [x] T001 `src/Bootstrapper/Sgt.DemoData/Sgt.DemoData.csproj` oluştur (BuildingBlocks + modül Application/Contracts referansları; Infrastructure yok); `Sgt.slnx`'e ve `src/Bootstrapper/Sgt.Api/Sgt.Api.csproj`'a ekle
- [x] T002 [P] Mimari testine `Sgt.DemoData` kuralı ekle: hiçbir modülün `Infrastructure` ad alanına bağlı olamaz, `tests/Architecture/ArchitectureTests.cs`

## Faz 2: Temel (tüm hikâyeleri bloklar)
- [x] T003 [P] `DemoClock` birim testi (Set/Advance, geriye gitmez) `tests/Bootstrapper/Sgt.DemoData.Tests/DemoClockTests.cs`; sonra `src/Bootstrapper/Sgt.DemoData/DemoClock.cs`
- [x] T004 [P] `IOutboxDrainer` + `OutboxDrainer<TContext>` (ProcessBatch 0 dönene kadar); `AddOutbox` kaydeder, `src/BuildingBlocks/Sgt.BuildingBlocks.Infrastructure/Messaging/IOutboxDrainer.cs` ve `MessagingServiceCollectionExtensions.cs`
- [x] T005 [P] `SyntheticNames` (sabit tohum, TR/EN/RU/GB karışık isim listeleri, kart numarası) + determinizm testi, `src/Bootstrapper/Sgt.DemoData/SyntheticNames.cs`
- [x] T006 `DemoScale` (Full: 500/40/200/30 gün; Small: 20/4/8/3 gün) ve `IDemoStep` arayüzü, `src/Bootstrapper/Sgt.DemoData/`

## Faz 3: US1 Demo verisi (MVP)
Bağımsız test: Small ölçekle seed → günlük özet her gün dolu, oyuncu sayısı doğru, ikinci çalıştırma no-op.
- [x] T007 [US1] Başarısız entegrasyon testi `tests/Api/Sgt.Api.Tests/DemoData/DemoSeedTests.cs` (WebApplicationFactory + yerel PostgreSQL, Small ölçek)
- [x] T008 [P] [US1] `ConfigurationSeeder`: TRY bazlı EUR/GBP/USD kurları (`SetExchangeRateCommand`), `Steps/ConfigurationSeeder.cs`
- [x] T009 [P] [US1] `PlayerSeeder`: `RegisterPlayerCommand` + `IssuePlayerCardCommand`; %2 oyuncuya `AddPlayerRestrictionCommand`, `Steps/PlayerSeeder.cs`
- [x] T010 [P] [US1] `FloorSeeder`: masalar (`DefineTableCommand`; rulet/blackjack/poker/bakara, TRY/EUR) ve slotlar (`DefineSlotMachineCommand`), `Steps/FloorSeeder.cs`
- [x] T011 [P] [US1] `LoyaltySeeder`: `SetEarningRuleCommand`, `Steps/LoyaltySeeder.cs`
- [x] T012 [US1] `DaySimulator`: her gün için saat ilerlet; masa aç (`TableSessionCommand`), kasa vardiyası aç/kapat, çip satış/bozdurma, döviz, rating başlat/bitir, slot sayaç okuması (`EnterMeterReadingCommand`), comp talep+onay; gün sonu `IOutboxDrainer` boşalt, `Steps/DaySimulator.cs`
- [x] T013 [US1] `DemoDataGenerator`: idempotency (tenant'ta oyuncu varsa çık), adım sırası, süre logu, `DemoDataGenerator.cs`
- [x] T014 [US1] `Program.cs`: `--seed-demo` dalı (Production'da çıkış kodu 2; DemoClock kaydı; migrate → seed → çık), `src/Bootstrapper/Sgt.Api/Program.cs`
- [x] T015 [US1] T007 yeşil; Full ölçek süre ölçümü < 2 dk (KK8)

## Faz 4: US2 Demo kullanıcıları
Bağımsız test: her demo kullanıcısının menüsü rolüne uygun (API testi, sahte JWT ile).
- [x] T016 [US2] Başarısız test: 6 demo kullanıcısı ve rolleri, `tests/Api/Sgt.Api.Tests/DemoData/DemoUsersTests.cs`
- [x] T017 [US2] `IdentitySeeder`: `CreateDefaultRolesCommand`; "Kasiyer" (cage.*) ve "Host" (loyalty/player) rolleri `CreateRoleCommand`+`GrantRolePermissionCommand`; `RegisterUserCommand`+`AssignRoleCommand`; sabit UUID'ler `DemoUsers.cs`
- [x] T018 [US2] `deploy/keycloak/sgt-realm.json`'a aynı UUID'lerle demo kullanıcıları; şifre `${SGT_DEMO_PASSWORD}`; `docker-compose.yml` env geçişi

## Faz 5: US3 Tek komut
- [x] T019 [US3] `build/demo.sh up|down|reset` (bash, `set -euo pipefail`; `.env` yoksa `openssl rand` ile demo değerleri üret, OTP CONDITIONAL; compose `--profile api`; `docker compose run api --seed-demo`; adres + kullanıcı listesi yazdır)
- [x] T020 [P] [US3] `deploy/.env.example`'a `SGT_DEMO_PASSWORD`, `docs/kurulum.md`'ye "Demo ortamı" bölümü

## Faz 6: Bitirme
- [x] T021 `dotnet build Sgt.slnx` uyarısız, `dotnet test Sgt.slnx` yeşil
- [x] T022 [P] `code-rules-reviewer`, `finans-denetci` (kasa/puan akışı), `veri-sagligi-denetci` raporları temiz
- [x] T023 spec durumunu "Tamamlandı" yap; quickstart adımlarını doğrula

## Bağımlılıklar
Faz1 → Faz2 → US1 → (US2 ∥ US3) → Bitirme. US2'nin IdentitySeeder'ı US1 komutlarının aktörü olduğu için US1'in DaySimulator'ı süper admin aktörüyle başlar; demo kullanıcıları sonra eklenir.

## Paralel örnek
T003, T004, T005 birlikte; T008–T011 birlikte (farklı dosyalar).

## Strateji
MVP = US1 (veri dolu sistem). Sonra US2, US3.
