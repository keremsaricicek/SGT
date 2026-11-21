# Sözleşme: demo komutları

| Komut | Davranış | Çıkış |
|---|---|---|
| `build/demo.sh up` | `.env` yoksa üretir; compose up (postgres, keycloak, api); `--seed-demo` çalıştırır | 0 başarı; adresleri ve demo kullanıcılarını yazar |
| `build/demo.sh down` | compose down (veri kalır) | 0 |
| `build/demo.sh reset` | compose down -v; sonra `up` | 0 |
| `dotnet run --project src/Bootstrapper/Sgt.Api -- --seed-demo` | migrate + demo verisi; Production'da hata kodu 2 | 0 / 1 hata / 2 reddedildi |

Yapılandırma: `DemoSeed:TenantId`, `DemoSeed:Scale` (`Full` | `Small`). HTTP API sözleşmesi (`openapi-v1.json`) değişmez.
