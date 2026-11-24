# ADR-0006: API sürümleme ve OpenAPI sözleşmesi

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Karar
- Tüm uçlar `/api/v1/...` altındadır. Kırıcı değişiklik yeni sürümle (`/api/v2`) yapılır; eski sürüm geçiş süresince yaşar.
- Sözleşme `docs/api/openapi-v1.json` dosyasıdır. API testleri yayınlanan dokümanı bu dosyayla karşılaştırır;
  fark varsa test kırmızıdır. Bilinçli değişiklik `SGT_UPDATE_OPENAPI=1 dotnet test` ile dosyaya yazılır ve PR'da incelenir.
- İstemciler (web, mobil, Windows) bu dosyadan istemci kodu üretir.
- OpenAPI dokümanı üretim ortamında yayınlanmaz.
