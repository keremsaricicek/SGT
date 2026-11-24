# ADR-0007: Dağıtım, veritabanı güncellemesi ve Windows servisi

- Tarih: 26.10.2026 · Durum: Kabul edildi

## Karar
- Üretimde API açılışta şemayı değiştirmez. Dağıtım adımı: `Sgt.Api --migrate` tüm modüllerin migration'larını uygular ve çıkar;
  ardından API normal başlatılır. Geliştirme ortamında API açılışta migration ve başlangıç verisini kendisi uygular.
- Migration'dan önce `pg_dump` yedeği alınır (docs/kurulum.md).
- API Windows sunucuda Windows servisi olarak çalışabilir (`UseWindowsService`, servis adı "SGT API");
  Linux'ta veya konteynerde normal süreçtir.
