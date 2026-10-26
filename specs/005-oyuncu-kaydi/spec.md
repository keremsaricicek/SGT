# Spec 005: Oyuncu kaydı, kartı, arama ve kısıtlama

Modül: CasinoPlayer · Ürün: S Plus · Durum: Onaylandı (koordinatör, 26.10.2026)
Ekranlar: player.search, player.profile, player.restrictions (docs/s-plus-ekranlar.md)

## Amaç
Oyuncu bir kez kaydedilir, kartla tanınır, hızla bulunur. Yasaklı oyuncu kasada ve masada işlem göremez.
Kişisel veri en az düzeyde tutulur ve korunur.

## Kabul kriterleri
- KK1: `player.profile:create` yetkilisi oyuncu kaydeder: ad, soyad, doğum tarihi, uyruk (ISO 2 harf),
  kimlik belgesi türü ve numarası, isteğe bağlı telefon. 18 yaşından küçük kaydedilemez.
- KK2: Aynı belge türü + numarası ile ikinci oyuncu kaydedilemez.
- KK3: Her oyuncuya tenant içinde artan, okunabilir bir oyuncu numarası verilir (ör. P000001).
- KK4: Belge numarası ve telefon veritabanında şifreli tutulur; ekranda belge numarasının yalnızca son 4 hanesi görünür.
  Belge numarasıyla arama, şifreyi çözmeden (kör indeks) yapılır.
- KK5: `player.search:view` yetkilisi oyuncu numarası, kart numarası, belge numarası (tam eşleşme) veya
  ad/soyad başlangıcıyla arar; sonuç en çok 50 kayıttır ve yalnızca özet bilgi içerir.
- KK6: `player.profile:edit` yetkilisi ad, soyad, telefon ve uyruğu düzeltir; doğum tarihi ve belge değişikliği de
  aynı yetkiyle yapılır ve KK1/KK2 kuralları yeniden uygulanır.
- KK7: `player.profile:issue_card` yetkilisi kart verir; kart numarası tenant içinde benzersizdir. Oyuncunun aynı anda
  tek aktif kartı olur; yeni kart verilince eskisi iptal edilir. `cancel_card` aktif kartı iptal eder.
- KK8: `player.restrictions:add` yetkilisi kısıtlama ekler (tür: Yasaklı, Kendini dışlama; gerekçe zorunlu, bitiş tarihi isteğe bağlı).
- KK9: Kısıtlamayı kaldıran kişi, onu ekleyen kişi olamaz (dört göz); `player.restrictions:remove` gerekir.
- KK10: Aktif kısıtlaması olan oyuncu sorgusu "kısıtlı" döner; diğer modüller bunu
  `CasinoPlayer.Contracts.IPlayerStatusQuery` ile sorar.
- KK11: Kayıt, düzeltme, kart ve kısıtlama işlemleri denetime düşer; denetim özetinde kişisel veri bulunmaz
  (yalnızca oyuncu numarası).

## Kapsam dışı
Fotoğraf, belge taraması, otel misafiri ile birleştirme (Party, S Vista fazında).
