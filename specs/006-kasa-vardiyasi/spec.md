# Spec 006: Kasa vardiyası, çip ve döviz işlemleri

Modül: Finance · Ürün: S Plus · Durum: Kod tamam, **insan uzman (teknik lider) finans onayı bekliyor**
Ekranlar: cage.shift, cage.chips, cage.exchange, cage.summary

> Anayasa III gereği bu modül `finans` etiketi alır; teknik lider onaylamadan ana dala birleşmez.

## Amaç
Kasiyer vardiyasını açar, oyuncuya çip satar/bozdurur, döviz bozar ve vardiyayı sayımla kapatır.
Her kuruş çift taraflı, silinemez kayıtla izlenir; kapanışta beklenen ile sayılan arasındaki fark ortaya çıkar.

## Kavramlar
- **Çekmece (drawer):** vardiyanın kasası. Bakiye, araç (Nakit/Çip) ve para birimi bazında tutulur.
- **Defter kaydı:** her işlem, para birimi bazında toplamı sıfır olan kayıtlar üretir (çift taraflı).
  Döviz bozumunda iki para birimini dengelemek için kur takas (FX) hesabı kullanılır.

## Kabul kriterleri
- KK1: `cage.shift:open` yetkilisi, terminal adıyla vardiya açar ve açılış tutarlarını (araç + birim) girer.
  Bir kasiyerin aynı anda tek açık vardiyası olur; bir terminalde aynı anda tek açık vardiya olur.
- KK2: Çip satışı (`cage.chips:sell`): çekmecede nakit artar, çip azalır. Çip bozdurma (`cage.chips:redeem`): tersi.
  Çekmecede yeterli çip/nakit yoksa işlem reddedilir.
- KK3: Döviz bozumu (`cage.exchange:exchange`): oyuncudan yabancı para alınır, alış kuruyla TRY ödenir; ya da
  TRY alınıp satış kuruyla yabancı para verilir. Kur yoksa işlem yapılmaz. Oyuncuya ödenen TRY aşağı, oyuncudan
  alınan TRY yukarı yuvarlanır (2 hane). Kullanılan kur işleme kaydedilir.
- KK4: Tutarlar sıfırdan büyük ve en çok 2 ondalıklıdır; yalnızca TRY/EUR/GBP/USD. Para `decimal` ve birimiyle tutulur.
- KK5: Her işlem istemcinin ürettiği bir idempotency anahtarı taşır; aynı anahtar ikinci kez gelirse yeni hareket
  oluşmaz, ilk sonuç döner.
- KK6: Kısıtlı oyuncu (Spec 005 KK10) ile işlem yapılamaz; oyuncu bulunamazsa işlem yapılamaz.
- KK7: Defter kayıtları ve işlemler değiştirilemez, silinemez (veritabanı tetikleyicisi). Her işlem para birimi
  bazında dengelidir (toplam sıfır).
- KK8: Kapalı vardiyaya işlem yapılamaz. Vardiyayı yalnızca açan kasiyer işletir.
- KK9: `cage.shift:close` ile kapanışta her araç/birim için sayılan tutar girilir; sistem beklenen bakiyeyi ve farkı
  kaydeder. Fark varsa gerekçe notu zorunludur.
- KK10: `cage.summary:view` vardiyanın anlık beklenen bakiyesini ve işlem listesini gösterir.
- KK11: Vardiya açılışı, her işlem ve kapanış denetime düşer.

## Kapsam dışı (sonraki spec'ler)
Düzeltme/ters kayıt ve dört göz onayı (cage.adjustment), ana kasadan çekmeceye dolum/iade, marker/kredi, raporlar.
