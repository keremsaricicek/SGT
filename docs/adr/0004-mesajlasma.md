# ADR-0004: Modüller arası mesajlaşma

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Karar
- Başlangıçta harici mesaj kuyruğu (Kafka/RabbitMQ) **yok**. Integration event'ler her modülün kendi şemasındaki
  `outbox_messages` tablosuna iş verisiyle aynı transaction'da yazılır; host içindeki dağıtıcı (background service)
  olayları aynı süreçteki abonelere iletir.
- Tüketiciler idempotenttir: her modülün `inbox_messages` tablosu (EventId + handler) ikinci işlemi engeller.
- Hata: üstel bekleme (2^deneme sn, en çok 1 saat), 10 denemeden sonra dead-letter; operatör yeniden işletir.
- Çoklu sunucu güvenliği: dağıtıcı satırları `FOR UPDATE SKIP LOCKED` ile alır.
- Olay tipi adı kalıcıdır: yeniden adlandırılmaz; kırıcı değişiklik yeni sürüm tipidir (`...V2`).

## Ne zaman değişir
S Slot'un yüksek hacimli makine olayları veya modüllerin ayrı servislere bölünmesi ölçülmüş bir ihtiyaç
oluşturursa, aynı outbox tablosundan harici kuyruğa aktaran bir yayıncı eklenir (ADR ile). Modül kodu değişmez.
