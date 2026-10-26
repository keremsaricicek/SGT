# ADR-0005: Kişisel verinin korunması ve oyuncu modülü

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Karar
- Oyuncu verisi `CasinoPlayer` modülündedir. Anayasadaki ayrı `Party` modülü (otel misafiri + oyuncu ortak kimliği)
  S Vista fazında çıkarılır; o zamana kadar oyuncunun kişisel verisi bu modülde tutulur.
- Hassas alanlar (belge numarası, telefon) uygulama katmanında AES-256-GCM ile şifrelenir (`IPersonalDataProtector`).
- Belge numarasıyla arama ve tekillik kontrolü HMAC-SHA256 kör indeksiyle yapılır; düz metin saklanmaz.
- Anahtarlar `Security:PersonalDataKey` ve `Security:BlindIndexKey` (base64, 32 bayt) yapılandırmadan gelir; üretimde
  secret manager'dadır. Depodaki anahtarlar yalnızca geliştirme içindir. Anahtar döndürme ayrı bir iş olarak planlanır.
- Loglara, denetim özetlerine ve olaylara kişisel veri yazılmaz; oyuncu numarası kullanılır.
