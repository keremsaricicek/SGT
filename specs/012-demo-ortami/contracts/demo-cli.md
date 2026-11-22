# Sözleşme: demo komutları

| Komut | Davranış | Çıkış |
|---|---|---|
| `build/demo.sh up` | `.env` yoksa üretir (`SGT_DEMO_ENV=1`); işaretsiz `.env` ile reddeder; compose up (postgres, keycloak), demo kullanıcılarını Keycloak'a ekler, `--seed-demo` çalıştırır, api'yi açar | 0 başarı; adresleri ve demo kullanıcılarını yazar |
| `build/demo.sh down` | compose down (veri kalır) | 0 |
| `build/demo.sh reset --yes` | compose down -v; sonra `up` (`--yes` olmadan reddeder) | 0 / 1 |
| `dotnet run --project src/Bootstrapper/Sgt.Api -- --seed-demo` | yalnızca Development/Demo + `DemoSeed:Enabled=true` + `DemoSeed:TenantId`; migrate + demo verisi + tamamlanma işareti | 0 / 1 hata (işaretsiz veri, kilit, işlenemeyen olay) / 2 reddedildi |

Yapılandırma: `DemoSeed:Enabled`, `DemoSeed:TenantId` (zorunlu), `DemoSeed:Scale` (`Full` | `Small`). HTTP API sözleşmesi (`openapi-v1.json`) değişmez.
