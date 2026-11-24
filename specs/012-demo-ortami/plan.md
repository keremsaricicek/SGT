# Plan: Demo ortamı (Spec 012, A2)

**Dal:** `faz0/proje-paketi` · **Spec:** [spec.md](spec.md) · **Tarih:** 21.11.2026

## Özet
API host'una `--seed-demo` bayrağı eklenir. Bayrak, yeni `Sgt.DemoData` projesindeki `DemoDataGenerator`'ı çalıştırır.
Üretici, modüllerin DI'daki `ICommandHandler<,>`'larını çağırır; geçmiş zaman `DemoClock` ile (IClock yerine) gün gün
ilerletilir; her simüle gün sonunda tüm modül outbox'ları eşzamanlı boşaltılır (puan kazanımı vb.). `build/demo.sh`
compose'u, `.env` üretimini ve yüklemeyi tek komutta toplar.

## Teknik bağlam
- .NET 10, EF Core 10, PostgreSQL 16, Keycloak 26.7 (mevcut `deploy/docker-compose.yml`).
- Yeni proje: `src/Bootstrapper/Sgt.DemoData` (yalnız Bootstrapper `Sgt.Api` referans verir).
- Bağımlılık: modül `Application` (komut kayıtları) + `Contracts` + BuildingBlocks. Infrastructure/DbContext'e erişim yok.
- Test: `tests/Api/Sgt.Api.Tests` altında entegrasyon testi (yerel PostgreSQL, `SGT_TEST_DB`) + `DemoClock`, isim üretici birim testleri.
- Performans hedefi: tüm yükleme < 2 dk (KK8).

## Constitution Check
| İlke | Uyum |
|---|---|
| I SOLID | Her veri türü ayrı üretici sınıf (`PlayerSeeder`, `FloorSeeder`, `CageDaySimulator`...), ortak `IDemoStep` arayüzü. |
| II Modülerlik | Yazımlar yalnızca modül komut handler'larıyla; tablo/DbContext erişimi yok. Arch testi `Sgt.DemoData`'yı da denetler. |
| III Değer bütünlüğü | Kasa hareketleri gerçek komutlarla, ledger kuralları aynen uygulanır; UPDATE/DELETE yok. `finans-denetci` incelemesi. |
| IV Tenant | Tek demo tenant'ı; tüm komutlar `TenantId` taşır. |
| V Test-önce | Entegrasyon testi önce yazılır: küçük ölçekli seed → günlük özet 30 gün dolu, ikinci çalıştırma no-op. |
| VI Güvenlik | Yalnız sentetik veri; Production'da bayrak reddedilir; demo şifreleri `.env`'de, repoda yok. |
| VII İzlenebilirlik | Her komut `AuditContext` (aktör, terminal `DEMO-SEED`, reason `DEMO`, correlation) taşır. |
| VIII Paketleme | Demo tenant'ı S Plus lisansıyla açılır (lisans kontrolü A4 ertelendi; mevcut lisans satırı kullanılır). |

Kapı: geçti, gerekçesiz ihlal yok.

## Yapı
```
src/Bootstrapper/Sgt.DemoData/
  DemoClock.cs                 # IClock; Set/Advance
  DemoDataGenerator.cs         # adımları sırayla çalıştırır, idempotency kontrolü
  DemoScale.cs                 # 500/40/200/30 (testte küçültülür)
  SyntheticNames.cs            # sabit tohumlu isim/kart üretici
  Steps/ (IdentitySeeder, ConfigurationSeeder, PlayerSeeder, FloorSeeder, LoyaltySeeder, DaySimulator)
src/BuildingBlocks/Sgt.BuildingBlocks.Infrastructure/Messaging/IOutboxDrainer.cs   # modül-bağımsız boşaltma
src/Bootstrapper/Sgt.Api/Program.cs   # --seed-demo dalı
build/demo.sh                          # up | down | reset
deploy/keycloak/sgt-realm.json         # demo kullanıcıları (sabit sub)
```

## Faz çıktıları
- [research.md](research.md) · [contracts/demo-cli.md](contracts/demo-cli.md) · [quickstart.md](quickstart.md)
- data-model: yeni kalıcı varlık yok (yalnız mevcut modül varlıkları üretilir); ayrı dosya gerekmedi.

## Post-design Constitution Check
Değişmedi; `IOutboxDrainer` BuildingBlocks'a eklenir (modül iç koduna bağımlılık yaratmaz).
