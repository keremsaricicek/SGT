# Spec 009: Slot makineleri ve sayaç verisi

Modül: GamingFloor · Ürün: S Plus · Durum: Tamamlandı (26.10.2026)
Ekranlar: slot.machines_setup, slot.manual_entry, slot.floor, slot.machine

## Amaç
Slot ekranları her zaman SGT'nindir; veri tek bir sözleşmeden gelir (docs/kararlar.md bölüm 3). Bu spec,
veri kaynağından bağımsız sayaç modelini ve ilk iki kaynağı kurar: elle giriş ve CSV içe aktarma
(NEXIO'nun dışa aktarım dosyaları da bu yoldan alınır). NEXIO API adaptörü ve S Slot aynı modele yazacaktır.

## Kabul kriterleri
- KK1: `slot.machines_setup:define` yetkilisi makine tanımlar: varlık numarası (tenant içinde benzersiz), alan/sıra,
  üretici, model, kupür (> 0) ve para birimi.
- KK2: Sayaç okuması makine sayaçlarının birikimli değeridir: giren (coin in), çıkan (coin out), jackpot ve oyun sayısı;
  hepsi ≥ 0, tutarlar en çok 2 ondalıklı. Okuma, kaynağını (Manual, Import, Nexio, SSlot) ve kaynak referansını taşır.
- KK3: Bir okuma, aynı makinenin önceki okumasından küçük sayaç içeremez; içeriyorsa yalnızca "sayaç sıfırlandı"
  işaretiyle ve açıklamayla kabul edilir. Okuma zamanı önceki okumadan sonra olmalıdır.
- KK4: Aynı kaynak + kaynak referansı ikinci kez gelirse yeni okuma oluşmaz (idempotent). Okumalar değiştirilemez, silinemez.
- KK5: İki okuma arasındaki faaliyet farklarla hesaplanır; makine kazancı = giren − çıkan − jackpot. Sıfırlamadan
  sonraki ilk okumada fark, yeni değerin kendisidir.
- KK6: `slot.manual_entry:enter` tek okuma girer; `slot.manual_entry:import` CSV yükler
  (`asset_number,read_at,coin_in,coin_out,jackpot,games_played[,reference]`). Her satır ayrı değerlendirilir; sonuç satır
  bazında başarılı/hatalı listesidir.
- KK7: `slot.floor:view` makine listesini son okuma zamanı ve iş günü (KKTC saati) toplam giren/kazanç ile gösterir.
  `slot.machine:view` makinenin son okumalarını ve farklarını gösterir.
- KK8: Makine tanımı, elle giriş ve içe aktarma özeti denetime düşer.

## Kapsam dışı
NEXIO API adaptörü (Advansys erişimi bekleniyor), S Slot (SMIB), makine olayları (kapı, hata), oyuncu slot oturumu.
