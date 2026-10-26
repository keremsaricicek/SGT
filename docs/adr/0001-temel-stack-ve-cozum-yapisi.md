# ADR-0001: Temel stack ve çözüm yapısı

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Karar

- Backend: .NET 10 / ASP.NET Core, tek host (`src/Bootstrapper/Sgt.Api`), modüler monolit.
- Kök ad alanı: `Sgt` (eski yer tutucu `Platform` kaldırıldı). Modüller: `Sgt.Modules.{Modül}.{Katman}`.
- Ortak yapı taşları: `src/BuildingBlocks/Sgt.BuildingBlocks` (`Result`, `Error`, `Money`, `IClock`).
- Test: xUnit; mimari kurallar NetArchTest ile `tests/Architecture`.
- Derleme: uyarı = hata, `latest-recommended` analizörler. Bilinçli istisnalar `.editorconfig`'te gerekçesiyle durur.
- Çözüm dosyası: `Sgt.slnx`.

## Açık bırakılanlar (ayrı ADR ile)

Veritabanı erişimi (EF Core + PostgreSQL), mesajlaşma (Kafka/RabbitMQ), kimlik (OIDC sağlayıcı),
Windows istemci (WPF/WinUI), mobil state yönetimi.
