# Spec 010: Kasa düzeltmesi (ters kayıt, dört göz)

Modül: Finance · Ürün: S Plus · Durum: Kod tamam, **insan uzman finans onayı bekliyor**
Ekran: cage.adjustment (request, approve)

## Kabul kriterleri
- KK1: `cage.adjustment:request` yetkilisi, açık bir vardiyadaki bir işlem için gerekçe notuyla düzeltme ister.
  Düzeltme yalnızca ters kayıttır: işlemin tüm defter satırları ters işaretle yeniden yazılır; asıl işlem değişmez.
- KK2: İsteği yapan kişi onaylayamaz; `cage.adjustment:approve` yetkilisi başka biri onaylar veya reddeder.
- KK3: Bir işlem yalnızca bir kez ters çevrilir; ters kayıt ters çevrilemez (veritabanında da benzersiz).
- KK4: Vardiya kapalıysa ters kayıt yapılamaz; çekmecede ters kayıt için yeterli nakit/çip yoksa onay reddedilir.
- KK5: Ters kayıt, asıl işlemin tutarını, kurunu ve oyuncusunu taşır; çekmece bakiyesi asıl işlemden önceki haline döner.
- KK6: İstek, onay/ret ve ters kayıt denetime düşer.
