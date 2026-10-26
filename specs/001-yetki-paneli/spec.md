# Spec 001: Yetki paneli (ekran bazlı yetki)

Modül: Identity · Ürün: Çekirdek (tüm ürünler) · Durum: Tamamlandı (26.10.2026)
Kaynak: `docs/kararlar.md` bölüm 4, `docs/s-plus-ekranlar.md`.

## Amaç
Kimin hangi ekranı görebileceği ve hangi işlemi yapabileceği koda gömülmeden, bir panelden yönetilir.
Her değişiklik silinemez şekilde kayda geçer.

## Kavramlar
- **Yetki kodu:** `<ekran>:<eylem>`. Eylem `view` (görebilir), `operate` (işlem yapabilir) veya bir alt
  işlem (ör. `cage.adjustment:approve`).
- **Rol:** yetki kodlarından oluşan küme. Bir kullanıcının birden fazla rolü olabilir.
- **Kişiye özel ekleme / çıkarma:** rolden bağımsız olarak kullanıcıya tek tek yetki eklenir veya çıkarılır.

## Kullanıcı hikâyeleri
- Süper admin olarak rol oluşturup ekran yetkilerini vermek istiyorum, böylece personel işini yapabilsin.
- Süper admin olarak bir kişiye rolünden farklı yetki ekleyip çıkarmak istiyorum.
- Personel olarak yalnızca görme yetkim olan ekranları menümde görmek istiyorum.
- Yönetici olarak yetki değişiklik geçmişini görmek istiyorum (kim, kime, ne zaman, ne).

## Kabul kriterleri
- KK1: Bir ekranın `view` dışındaki yetkisi, aynı ekranın `view` yetkisi olmadan verilemez.
- KK2: Bir role/kişiye `view` yetkisi, o ekranın diğer yetkileri dururken geri alınamaz.
- KK3: Etkin yetki = (rollerin yetkileri ∪ kişiye eklenenler) − kişiden çıkarılanlar. Çıkarılan `view`,
  o ekranın tüm yetkilerini de etkisiz kılar.
- KK4: Menü yalnızca etkin `view` yetkisi olan **ve** lisansı açık üründeki ekranları içerir.
- KK5: Rol ve yetki değişikliğini yalnızca süper admin yapabilir; diğerleri `Identity.NotSuperAdmin` hatası alır.
- KK6: Her değişiklik bir değişiklik kaydı üretir: aktör, hedef (rol/kişi), yetki, ekleme/çıkarma, zaman, gerekçe kodu.
  Kayıtlar değiştirilemez ve silinemez.
- KK7: Aynı yetkiyi tekrar vermek veya olmayan yetkiyi geri almak hata değildir, ama kayıt üretmez.
- KK8: Rol ve kullanıcı yetkileri tenant kapsamındadır; başka tenant'ın rolü atanamaz.
- KK9: Varsayılan rol şablonları (slot müdürü, pit boss, casino müdürü, otel genel müdürü, otel personeli)
  hazır gelir ve değiştirilebilir.
- KK10: Yetki kontrolü sunucuda yapılır; istemci menüsü yalnızca kolaylıktır.
- KK11: Süper admin, yönetim (Core) ekranlarının tüm yetkilerine rol atanmadan sahiptir; diğer ekranlar için
  normal kurallar geçerlidir.
- KK12: Her değişiklik isteği bir gerekçe kodu taşır; gerekçesiz istek reddedilir.

## Kapsam dışı
- Giriş/şifre/MFA (ayrı spec; OIDC sağlayıcı ADR'si bekleniyor).
- Web panelinin arayüzü (istemci fazında).

## Açık sorular
- Yok. Ekran kodlarının tam listesi `docs/s-plus-ekranlar.md`'den üretilecek.
