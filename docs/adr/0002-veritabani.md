# ADR-0002: Veritabanı ve kalıcılık

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Karar

- PostgreSQL 16+, EF Core 10 (Npgsql). Tablo/kolon adları snake_case.
- Her modülün kendi şeması ve kendi DbContext'i vardır (ör. `identity`). Modüller arası join yoktur (K2).
- Domain nesneleri EF'e doğrudan bağlanmaz: Infrastructure'da satır sınıfları (`*Row`) ve eşleme vardır;
  aggregate'ler `Restore(...)` ile geri yüklenir. Böylece domain'de public setter olmaz (K1).
- Değer/denetim kayıtları (ör. `identity.permission_changes`) append-only'dir: veritabanı tetikleyicisi
  UPDATE ve DELETE'i reddeder.
- Migration: `dotnet ef`, modül başına `Persistence/Migrations`. Geçmiş tablosu modül şemasında `__ef_migrations`.
- Entegrasyon testleri gerçek PostgreSQL'de, test başına geçici veritabanıyla çalışır (`SGT_TEST_DB`).
