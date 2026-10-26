# Spec 007: Canlı oyun — masalar, masa oturumu, oyuncu derecelendirme

Modül: GamingFloor · Ürün: S Plus · Durum: Tamamlandı (26.10.2026)
Ekranlar: live.tables, live.table_session, live.rating, player.locator

## Amaç
Pit ekibi masaları açıp kapatır, masadaki oyuncunun oyununu derecelendirir (rating). Rating, loyalty ve
raporların temelidir; "oyuncu şu an nerede" sorusunu da yanıtlar.

## Kabul kriterleri
- KK1: `live.tables:define` yetkilisi masa tanımlar: kod (tenant içinde benzersiz, ör. BJ-01), oyun türü
  (Blackjack, Rulet, Poker, Bakara, Diğer), para birimi, min/maks bahis, kasa avantajı (%) ve saatlik el sayısı.
  Min > 0, min ≤ maks, kasa avantajı 0–100 arası.
- KK2: `live.table_session:open` masayı açar; bir masanın aynı anda tek açık oturumu olur.
  `close` masayı kapatır; masada süren rating varken kapatılamaz.
- KK3: `live.rating:start` açık masada, kısıtlı olmayan bir oyuncu için rating başlatır. Bir oyuncunun aynı anda
  tek süren rating'i olur.
- KK4: `live.rating:finish` rating'i bitirir: ortalama bahis (masa para biriminde, masa limitleri içinde) ve
  isteğe bağlı oyuncu sonucu (kazanç + / kayıp −) girilir. Süre başlangıç–bitişten hesaplanır.
- KK5: Teorik kazanç = ortalama bahis × saatlik el × saat × kasa avantajı; 2 haneye yuvarlanır ve kaydedilir.
- KK6: `live.rating:correct` bitmiş rating'in ortalama bahis ve sonucunu düzeltir; eski değerler silinmez,
  düzeltme gerekçesiyle ayrı kayıt olur, rating'in geçerli değeri son düzeltmedir.
- KK7: `player.locator:view` yetkilisi, oyuncunun süren rating'i varsa masa kodunu ve başlangıç zamanını görür;
  diğer modüller bunu `GamingFloor.Contracts.IPlayerLocationQuery` ile sorar.
- KK8: Biten ve düzeltilen rating'ler `PlayerRatedV1` olayıyla yayınlanır (loyalty için); tüm işlemler denetime düşer.

## Kapsam dışı
Masa çip float/fill/credit (live.chips), drop (live.drop), slot rating'i.
