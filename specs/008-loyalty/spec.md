# Spec 008: Loyalty — puan, seviye, puan düzeltme ve comp

Modül: Loyalty · Ürün: S Plus · Durum: Kod tamam · Değer hareketi: **insan uzman finans onayı bekliyor**
Ekranlar: loyalty.points, loyalty.comp, loyalty.rules

## Amaç
Oyuncu oynadıkça puan kazanır, seviyesi yükselir; puanla ikram (comp) alır. Puan da değerdir:
kayıtlar silinmez, elle değişiklik ve harcama iki kişinin onayıyla yapılır.

## Kabul kriterleri
- KK1: Biten rating (`PlayerRatedV1`) puan kazandırır: puan = aşağı yuvarla(TRY teorik kazanç / kural oranı).
  Kural oranı varsayılan 100 TRY = 1 puandır; `loyalty.rules:define` ile değiştirilir (> 0).
  TRY dışı rating, bitiş anındaki alış kuruyla TRY'ye çevrilir; kur yoksa olay yeniden denenir (operatör görür).
- KK2: Rating düzeltmesi (`IsCorrection`) puanı yeniden hesaplar ve yalnızca farkı kaydeder; aynı olay iki kez puan vermez.
- KK3: Puan kayıtları değiştirilemez, silinemez; bakiye kayıtların toplamıdır ve hiçbir işlem bakiyeyi eksiye düşüremez.
- KK4: Seviye, toplam kazanılan puana göre belirlenir: Bronz 0, Gümüş 1.000, Altın 5.000, Platin 20.000.
- KK5: Puan düzeltme `loyalty.points:adjust` ile istenir (+/−, açıklama zorunlu) ve **başka bir** `adjust` yetkilisi
  onaylayınca veya reddedince sonuçlanır.
- KK6: Comp `loyalty.comp:request` ile istenir (puan > 0, açıklama zorunlu); **başka bir** `loyalty.comp:approve`
  yetkilisi onaylayınca puan düşülür. Bakiye yetmezse onay reddedilir. Kısıtlı oyuncu comp isteyemez.
- KK7: `loyalty.points:view` yetkilisi oyuncunun bakiyesini, seviyesini, son hareketlerini ve bekleyen isteklerini görür.
  Diğer modüller seviyeyi `Loyalty.Contracts.IPlayerTierQuery` ile sorar.
- KK8: Tüm işlemler denetime düşer.
