# Spec 013: Ortak tasarım dili (A11)

**Dal**: `faz0/proje-paketi` üzerinden çalışma dalı · **Oluşturuldu**: 21.11.2026 · **Durum**: Uygulandı (web)
Ürün: tüm ürün ailesi (S Plus, S Slot, S Vista) · İstemciler: web (bu spec), Windows ve mobil (sonraki aşamalar)
Karar kaynağı: `docs/kararlar.md` §9a "Ortak tasarım dili (A11)"

**Girdi**: Kurumsal, işlev önce; klasik masaüstü iş uygulaması düzeni (menü/yan menü, araç çubuğu, yoğun grid,
etiketli alanlar, durum çubuğu, klavye kısayolları) modernleştirilmiş. Hap/oval düğme yok, köşe yarıçapı en çok 3px,
süs animasyonu/gradyan yok. Koyu (varsayılan) ve açık tema, seçim kalıcı. Tek kaynak token dosyası; web, Windows ve
mobil aynı değerleri kullanır. WCAG AA kontrast iki temada.

## Amaç

Casino personeli yıllardır klasik Windows iş uygulamalarıyla çalışıyor: üstte uygulama çubuğu, solda menü, ekranın
başında araç çubuğu, yoğun tablolar, altta durum çubuğu. Yeni ürün bu alışkanlığı bozmadan daha okunur, daha düzenli
ve daha sakin görünmelidir. Aynı görünüm web, Windows ve mobilde tek bir kaynaktan (tasarım token dosyası) beslenir;
böylece bir renk ya da ölçü bir kez değişir, üç istemci aynı kalır. Bu bir güzellik yarışması değildir: her karar
okunurluk, hız ve hata yapmama için verilir.

## Kullanıcı senaryoları ve testler

### Senaryo 1 — Tanıdık düzende çalışma (P1)

Kasa/oyuncu/yönetim personeli panele girer; solda gruplanmış menü, üstte uygulama çubuğu (ürün adı, terminal,
kullanıcı), her ekranın başında başlık ve işlem düğmeleri olan bir araç çubuğu, altta durum çubuğu görür.
Tablolar yoğundur, sütun başlıkları sabit, sayılar sağa hizalı ve eşit genişlikli rakamlarladır.

**Neden P1**: Personelin yeni ürüne geçişte kaybolmaması ve hızlı çalışması için temel koşul.

**Bağımsız test**: Herhangi bir liste ekranı açılır; düzen öğelerinin hepsi görünür, satır yüksekliği ölçülür.

**Kabul senaryoları**:
1. **Verilen** oturum açık, **ne zaman** herhangi bir ekran açılır, **o zaman** uygulama çubuğu, yan menü, ekran araç
   çubuğu (başlık + işlemler) ve durum çubuğu aynı yerde görünür.
2. **Verilen** bir liste ekranı, **ne zaman** satırlar listelenir, **o zaman** satır yüksekliği standart yoğunlukta
   en çok 28 px, sıkı yoğunlukta en çok 24 px'tir; tutar sütunları sağa hizalıdır.
3. **Verilen** kısıtlı bir oyuncu, **ne zaman** arama sonucunda görünür, **o zaman** satırda kısıtlı durum rengiyle
   işaret ve "Kısıtlı" rozeti vardır (yalnızca renge dayanmaz).

### Senaryo 2 — Koyu/açık tema (P1)

Casino salonunda ekran karanlık ortamda kullanılır; varsayılan tema koyudur. Arka ofiste çalışan personel açık temaya
geçebilir; seçim bir sonraki açılışta korunur.

**Bağımsız test**: Tema değiştirilir, sayfa yenilenir, seçim korunur.

**Kabul senaryoları**:
1. **Verilen** ilk kez açılan panel, **ne zaman** sayfa yüklenir, **o zaman** koyu tema etkindir.
2. **Verilen** koyu tema, **ne zaman** kullanıcı durum çubuğundan açık temayı seçer, **o zaman** tüm ekran anında açık
   temaya geçer; sayfa yenilendiğinde açık tema kalır.
3. **Verilen** herhangi bir tema, **ne zaman** sayfa ilk kez çizilir, **o zaman** yanlış temada yanıp sönme olmaz.

### Senaryo 3 — Klavyeyle çalışma (P2)

Kasiyer fareye uzanmadan ekranlar arasında geçer ve formları doldurur.

**Kabul senaryoları**:
1. **Verilen** yan menüde N ekran, **ne zaman** Alt+1…Alt+9 basılır, **o zaman** sıradaki ekran açılır; kısayol
   menüde yazılıdır.
2. **Verilen** herhangi bir etkileşimli öğe, **ne zaman** Tab ile odaklanılır, **o zaman** en az 2 px görünür odak
   çerçevesi vardır.
3. **Verilen** açık bir diyalog, **ne zaman** Esc basılır, **o zaman** diyalog kapanır ve odak açan düğmeye döner.
4. **Verilen** sekmeli bir ekran, **ne zaman** sekme listesinde sol/sağ ok basılır, **o zaman** sekmeler arasında geçilir.

### Senaryo 4 — Tek kaynak token (P2)

Tasarımcı veya geliştirici bir rengi tek dosyada değiştirir; web CSS değişkenleri komutla yeniden üretilir. Windows
(WPF) ve mobil (Flutter) aynı dosyayı kendi biçimlerine çevirecektir.

**Kabul senaryoları**:
1. **Verilen** token dosyası, **ne zaman** üretim komutu çalışır, **o zaman** web tema değişkenleri dosyadan birebir
   üretilir; elle değişiklik yapılmaz.
2. **Verilen** token dosyası ile üretilmiş dosya farklı, **ne zaman** testler çalışır, **o zaman** test başarısız olur.

### Uç durumlar

- Tarayıcı depolaması kapalıysa (gizli pencere) tema varsayılan koyu kalır, hata vermez.
- Çok uzun metinler (ör. hata mesajı) tabloyu taşırmaz: yalnızca kaydırılabilir alan içinde kaydırılır ya da sarılır.
- Dar pencerede (≤ 760 px) yan menü üstte yatay şerit olur; içerik yatay sayfa kaydırması yapmaz.
- Negatif tutar yalnızca renkle değil, eksi işaretiyle de gösterilir.

## Gereksinimler

### İşlevsel gereksinimler

- **KK1 / FR-001 Token kaynağı**: Tek bir tasarım token dosyası (`design/tokens/sgt-tokens.json`) tema başına
  (koyu, açık) renkleri, tipografi ölçeğini, boşluk ölçeğini, köşe yarıçaplarını, kenarlıkları, grid yoğunluklarını,
  kontrol yüksekliklerini ve durum renklerini (para girişi, para çıkışı, uyarı, hata, başarı, bilgi, kısıtlı oyuncu)
  içerir. Değerler platformdan bağımsız birimlerle (px, onaltılık renk) yazılır.
- **KK2 / FR-002 Üretim**: Web tema değişkenleri token dosyasından bir komutla üretilir ve üretilmiş dosya, token
  dosyasıyla eşitliği testle denetlenir.
- **KK3 / FR-003 Biçim kısıtları**: Hiçbir düğme, alan, rozet veya panel 3 px'ten büyük köşe yarıçapı kullanmaz.
  Gradyan, cam efekti, gölgeli büyük kartlar ve süs animasyonu yoktur (yalnızca diyalog altındaki karartma ve odak
  çerçevesi gibi işlevsel görseller).
- **KK4 / FR-004 Kabuk**: Uygulama çubuğu (ürün adı, terminal, kullanıcı, çıkış), gruplanmış yan menü (Oyuncular,
  Raporlar, Yönetim; aktif öğe işaretli; kısayol yazılı), ekran araç çubuğu (başlık + işlemler) ve durum çubuğu
  (ekran kodu, terminal, kullanıcı, dil, tema, kısayol ipucu) her ekranda vardır.
- **KK5 / FR-005 Temel bileşenler**: Düğme (varsayılan, birincil, tehlike, sade; normal ve küçük boy), metin alanı,
  seçim alanı (etiket üstte, zorunlu işaretli), veri gridi (sabit başlık, sayı hizalama, boş durum, satır vurgusu,
  durum satırları), diyalog (modal, Esc ile kapanır), sekmeler (ok tuşlarıyla gezinme) ve rozet (nötr, başarı,
  uyarı, tehlike, kısıtlı, bilgi) ortak bileşen olarak bulunur; ekranlar bunları kullanır.
- **KK6 / FR-006 Tema**: Koyu varsayılan; açık tema seçilebilir; seçim tarayıcıda saklanır ve ilk çizimden önce
  uygulanır.
- **KK7 / FR-007 Kontrast**: Her iki temada normal metin/arka plan çiftleri en az 4.5:1, kontrol kenarlıkları, odak
  çerçevesi ve durum işaretleri en az 3:1 kontrast sağlar (WCAG 2.2 AA); bu testle denetlenir.
- **KK8 / FR-008 Para ve durum anlamı**: Para girişi ve para çıkışı ayrı token renkleriyle gösterilir; negatif tutar
  eksi işaretiyle ve para çıkışı rengiyle yazılır. Kısıtlı oyuncu satırı sol kenar işareti ve rozetle ayrılır.
- **KK9 / FR-009 Tipografi**: Tek yazı ailesi (sistem arayüz yazısı, Windows'ta Segoe UI) ve tek eşit aralıklı aile;
  gövde 13 px, tablo 13 px, başlık ölçeği en fazla 5 basamak; tüm tutar ve sayı sütunları eşit genişlikli rakam kullanır.
- **KK10 / FR-010 Klavye**: Alt+1…9 menü kısayolları, görünür odak, diyalogda Esc, sekmede ok tuşları.
- **KK11 / FR-011 Kapsam**: Web istemcisinin tüm ekranları (ana sayfa, giriş, oyuncu arama/detay, roller/rol detayı,
  kullanıcılar/kullanıcı erişimi, yetki geçmişi, kur, günlük rapor, denetim, entegrasyon) yeni dille biçimlenir.
  İşlevsel davranış (yetki, gerekçe kodu, API çağrıları) değişmez.
- **KK12 / FR-012 Kanıt**: Her ekranın koyu ve açık temada 1440×900 ekran görüntüsü (sentetik veriyle)
  `docs/ekran-goruntuleri/web/` altında bulunur.

### Ana varlıklar

- **Tasarım token'ı**: Ad (ör. `color.dark.surface`), değer, tür (renk, ölçü, yazı ailesi). Tema bağımlı ya da tema
  bağımsız olabilir.
- **Tema**: Koyu veya açık; kullanıcının cihazında saklanan tercih.

## Başarı ölçütleri

- **SC-001**: Token dosyasında tanımlı her metin/arka plan çifti iki temada da ≥ 4.5:1, her kontrol/durum işareti
  ≥ 3:1 kontrast verir (otomatik test, %100 geçer).
- **SC-002**: Arayüz biçim dosyalarında 3 px'ten büyük köşe yarıçapı, gradyan ve süs animasyonu sayısı 0'dır
  (otomatik test).
- **SC-003**: Standart grid satırı ≤ 28 px; 1440×900 ekranda bir liste ekranında en az 20 satır kaydırmadan görünür.
- **SC-004**: Tema seçimi sayfa yenilemesinden sonra %100 korunur.
- **SC-005**: Web istemcisinin 13 ekranının iki temadaki ekran görüntüsü (26 görsel) depoda bulunur.
- **SC-006**: Mevcut lint, tip kontrolü, birim testleri ve üretim derlemesi hatasız geçer.

## Varsayımlar

- Yazı tipi dışarıdan indirilmez (kurumsal ağlarda çevrimdışı çalışma, KVKK/gizlilik); sistem yazı tipi kullanılır.
- Windows (WPF) ve mobil (Flutter) istemcilerin token dosyasını kullanması bu spec'in kapsamında değildir; dosya
  yapısı bu dönüşümü kolaylaştıracak biçimde (düz, platformdan bağımsız değerler) tutulur.
- Kısayol olarak tarayıcının ayırdığı tuşlar (F5, Ctrl+R vb.) kullanılmaz.
- Lisans açma/kapama (A4) ertelendiği için menüde ürün bazlı gizleme yapılmaz; menü yalnızca yetkiye göre oluşur.
