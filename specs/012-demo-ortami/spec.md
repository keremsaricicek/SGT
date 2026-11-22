# Spec 012: Demo ortamı (A2)

Modül: Bootstrapper (tüm modüllerin komut handler'ları) · Ürün: S Plus · Durum: Tamamlandı (22.11.2026; `build/demo.sh` Docker yolu bulut oturumunda çalıştırılamadı, kullanıcı makinesinde doğrulanacak)

## Amaç
Proje sahibi ve satış, sistemi tek komutla açıp gerçekçi ama tamamen sahte veriyle dolu bir casino görür.
Veri, modüllerin kendi komut handler'larından geçerek yazılır (K2); hiçbir tabloya doğrudan yazılmaz.

## Kabul kriterleri
- KK1: `build/demo.sh up` tek komuttur: `deploy/.env` yoksa demo değerleriyle üretir (rastgele, yalnızca demo),
  PostgreSQL + Keycloak + API'yi açar, migration ve demo verisini yükler. `build/demo.sh down` kapatır,
  `build/demo.sh reset` veriyi silip yeniden yükler.
- KK2: API `--seed-demo` bayrağıyla demo verisini yükleyip çıkar. Production ortamında bayrak reddedilir.
- KK3: Veri: 500 oyuncu (kartlı; birkaçı kısıtlı), 40 masa, 200 slot makinesi, TRY/EUR/GBP/USD kurları,
  puan kazanma kuralı ve son 30 günün hareketi: kasa vardiyaları (açılış/kapanış, çip satış/bozdurma, döviz),
  rating'ler (puan kazanımı outbox ile Loyalty'ye geçer), slot sayaç okumaları, comp talep ve onayları.
- KK4: Geçmiş zamanlı hareket, demo saati (`IClock` yerine ileri sarılabilen saat) ile üretilir; günlük özet
  son 30 günün her günü için veri gösterir.
- KK5: Üretim deterministiktir (sabit tohum): aynı komut aynı veriyi üretir. Tenant'ta demo verisi varsa
  ikinci çalıştırma hiçbir şey yazmaz.
- KK6: Her rol için demo kullanıcısı: süper admin, casino müdürü, pit boss, slot müdürü, kasiyer, host.
  Keycloak'ta sabit `sub` ile tanımlıdır; demo ortamında OTP kapalıdır. Şifreler `deploy/.env`'dedir.
- KK7: Kişisel veri tamamen sentetiktir (isim listelerinden üretilir); gerçek kişi, kart veya üretim verisi yoktur.
- KK8: Yükleme 2 dakikadan kısa sürer (yerel makinede).

## Notlar
- Bilinen istisna: demo tenant'ının lisansı ve ilk süper admini `IdentityDevelopmentSeed` ile yazılır; bu geliştirme tohumu
  denetim kaydı (audit) üretmez. Yalnızca veri yokken ve yükleme gerçekten yapılacakken çalışır. Diğer tüm demo verisi
  modül komutlarıyla, denetim kaydıyla yazılır.
- Yükleme tamamlanınca Configuration modülünde `DEMO_DATA_V1` tenant işareti konur (KK5 idempotency); işaretsiz veri
  varsa yükleme 1 ile durur ve `reset --yes` önerilir.
- KK2 kapısı (inceleme sonrası): yalnızca `Development`/`Demo` ortamı + `DemoSeed:Enabled=true` + açık `DemoSeed:TenantId`;
  aksi 2.
