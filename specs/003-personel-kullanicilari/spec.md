# Spec 003: Personel kullanıcıları

Modül: Identity · Ürün: Çekirdek · Durum: Tamamlandı (26.10.2026)

## Amaç
Personel SGT'ye kayıtlı olmadan hiçbir ekranı kullanamaz; ayrılan personel tek işlemle tamamen kapatılır.
Giriş bilgisi (şifre, MFA) Keycloak'tadır (ADR-0003); SGT kullanıcısı Keycloak kimliğine (`sub`) bağlanır.

## Kabul kriterleri
- KK1: `admin.users:create` yetkilisi, Keycloak kimliği, ad soyad ve personel kodu ile kullanıcı oluşturur.
  Ad ve personel kodu boş olamaz; personel kodu tenant içinde benzersizdir.
- KK2: Aynı Keycloak kimliği ikinci kez kaydedilemez.
- KK3: `admin.users:deactivate` yetkilisi kullanıcıyı pasifleştirir veya yeniden aktifleştirir.
- KK4: Pasif kullanıcının hiçbir yetkisi yoktur (menü boş, tüm yetki kontrolleri olumsuz); rolleri ve özel
  yetkileri silinmez, aktifleşince geri gelir.
- KK5: Süper admini yalnızca süper admin pasifleştirebilir; tenant'taki son aktif süper admin pasifleştirilemez.
- KK6: Kullanıcı oluşturma, pasifleştirme ve aktifleştirme yetki değişiklik kaydına ve denetime düşer (gerekçe kodu zorunlu).
- KK7: `admin.users:view` yetkilisi kullanıcı listesini (ad, personel kodu, aktiflik, süper admin mi) görür.

## Kapsam dışı
Keycloak'ta kullanıcıyı otomatik açma (sonraki adım: Keycloak admin API adaptörü).
