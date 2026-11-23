# SGT — Karar Kaydı

Proje sahibiyle yapılan planlama görüşmesinde (25–26 Ekim 2026) alınan kararlar.
**Bu dosya fizibilite raporundan sonra gelir ve onunla çeliştiği her yerde geçerli olan budur.**
Fizibilite raporunu proje sahibi hazırlamadı; eksik veya bu kararlarla uyumsuz olabilir (bkz. bölüm 10).

---

## 1. Şirket ve ürünler

Şirket: **SGT — Sentez Gaming Technologies**

| Ürün | Ne yapar | Satış şekli |
|---|---|---|
| **S Plus** | Ana casino işletim sistemi. Her şey burada: oyuncular, oyuncu kartı/takibi, kasa (cage), loyalty, host/PR, canlı oyun (masa oyunları), slot ekranları, raporlar, yetki paneli. Birden fazla ürün alındığında hepsine tek yerden erişilen çatı uygulama da budur. | Temel ürün, tek başına satılır |
| **S Slot** | Slot makinelerinden anlık veri toplayan modül; NEXIO'nun yerine geçer. | Ayrı modül, isteğe bağlı |
| **S Vista** | Otel işletim sistemi; Opera/Fidelio/Elektra sınıfı PMS'in yerine geçer. | Ayrı modül, isteğe bağlı |

- "S Floor" adı kaldırıldı; canlı oyun S Plus'ın içindedir.
- Ürün adları için yerel (KKTC) marka araması yapılacak.
- Arka planda tüm ürünlerin kullandığı ortak çekirdek (kimlik, yetki, audit, konfigürasyon, entegrasyon) vardır;
  müşteriye ayrı ürün olarak görünmez.

## 2. İstemciler

- S Plus'ın ekranları pencereler gibi açılır (ör. slot penceresi, canlı oyun penceresi, kasa penceresi).
- İstemciler: Windows uygulaması (operasyon), akıllı telefon uygulaması (iOS/Android), web paneli.
- Her istemci aynı API'yi ve aynı yetki modelini kullanır. Hiçbir istemci veritabanına bağlanmaz.

## 3. Dış bağlantılar: tek bağlantı NEXIO

- Rakip/üçüncü taraf sistemlerden **yalnızca NEXIO'ya bağlanılır** ve bu bağlantı isteğe bağlıdır.
- **Slot verisi tek bir girişten gelir.** S Plus'ın slot ekranları her zaman SGT'nindir; veri kaynağı değişir:

| Müşterinin durumu | Slot verisinin kaynağı |
|---|---|
| NEXIO kullanıyor, yalnızca S Plus aldı | NEXIO adaptörü |
| S Slot'u da aldı | S Slot |
| İkisi de yok | Slot verisi yok (manuel giriş/import) |

- **Otel:** S Vista yalnızca SGT ürünlerine bağlanır. Müşteri kendi otel programını (Opera, Elektra vb.) kullanmak
  isterse o sistem SGT'ye bağlanmaz, ayrı kalır. Opera/Elektra için adaptör yazılmaz.
- Hiçbir bağlantı zorunlu değildir; müşteri NEXIO'yu da oteli de bağlamayabilir.

## 4. Roller ve yetkilendirme

- Yetkiler bir **yetki panelinden** verilir; koda gömülmez.
- **Her ekranın kendi yetki satırı** vardır: "görebilir" ve "işlem yapabilir" ayrı ayrı verilir. İşlem yetkisi
  gerektiğinde daha da bölünür (ör. kasada para girişi yapabilir ama düzeltme onaylayamaz).
- Her kombinasyon mümkündür: hepsi, hiçbiri, yalnızca resepsiyon, yalnızca canlı oyun vb.
- Görme yetkisi olmayan ekran kullanıcının menüsünde hiç görünmez.
- Yetkiler role veya kişiye verilir; role verilen yetki o roldeki herkese uygulanır, kişiye özel ekleme/çıkarma yapılabilir.
- Rol ve yetkiyi yalnızca süper admin verir. Her yetki değişikliği (kim, kime, ne zaman, ne) silinemez şekilde kaydedilir.

**Hazır gelen varsayılan rol şablonları** (işletme değiştirebilir):

| Rol | Slot | Canlı oyun | Otel (S Vista) | Not |
|---|---|---|---|---|
| Slot müdürü / supervisor | ✅ | ❌ | ❌ | |
| Pit boss / pit manager | ❌ | ✅ | ❌ | |
| Casino müdürleri | ✅ | ✅ | ✅ | Casino tarafında oteli görebilen tek rol |
| Otel genel müdürü | ✅ | ✅ | ✅ | Otel tarafında casinoyu görebilen tek rol |
| Otel personeli | ❌ | ❌ | ✅ (ilgili ekranlar) | |

- **Oyuncu arama istisnası:** Slot ve canlı oyun tarafı birbirinin ekranlarını görmez, ama "bu oyuncu şu an
  nerede oynuyor" sorusu için ortak oyuncu arama yetkisi verilebilir (yalnızca konum ve temel bilgi).

## 5. Pazar

- Tek pazar: **KKTC**.
- Proje sahibinin ifadesiyle hukuki/regülasyon engeli yoktur; GLI benzeri sertifika zorunluluğu varsayılmaz.
  (Vergi ve resmi raporlama yükümlülükleri bir mali müşavirle teyit edilecek.)
- Çoklu para birimi zorunlu: TRY, EUR, GBP, USD.
- Arayüz dilleri: Türkçe ve İngilizce.

## 6. S Slot teknik yaklaşımı

- Slot makinesinin içine yazılım kurulmaz (makine yazılımı kapalı/mühürlüdür; Linux veya Windows olması önemsizdir).
- Veri, makinedeki arayüz kartı (SMIB) üzerinden protokolle okunur: SAS (IGT, lisanslı) veya G2S (GSA, açık standart).
- Okunacak veri: oyun sayısı, oynanan tutar, kazanç, jackpot, kapı/hata olayları (sayaçlar ve olaylar).
- Oyuncu bazlı bilgi (o an kaç kez oynadı, ne kadar kaybetti) oyuncu kartı oturumu + makine sayaçlarının
  birleştirilmesiyle hesaplanır.
- **Açık karar:** mevcut SMIB/arayüz kartı üreticileriyle anlaşma mı, kendi donanımımız mı?
- Ofis bilgisayarları Windows'tur; SGT Windows uygulaması orada çalışır.

## 7. Özgünlük ve tersine mühendislik politikası

- Ürün özgün olacak; rakiplerin ekranları, kaynak kodu veya ticari tasarımı kopyalanmaz.
- Rakip ürünlerden özellik fikri **kullanarak** öğrenilir: lisanslı kullanım, demo, kılavuz, personelle görüşme.
- Rakip yazılımların kodu açılmaz/decompile edilmez (REA vb. araçlar bu amaçla kullanılmaz).
  REA yalnızca SGT'nin kendi yazılımları için kullanılabilir.

## 8. Kod kuralları

- **K1:** Kod SOLID ilkelerine göre yazılır; OOP kuralları esastır.
- **K2:** Uygulamadaki her işlev ayrı kodlanır ve diğerleriyle tanımlı sözleşmeler üzerinden entegre olur.
- Ayrıntı: `docs/kod-kurallari.md`. Listede eksik olan 2 numaralı madde kaldırıldı (26.10.2026); kod kuralları K1 ve K2'dir.

## 9. Diğer

- **Merit Event Studio** ayrı bir ürün ve ayrı bir depo olarak kalır; aynı kural paketi orada da kullanılabilir.
- Agent ve skill seçimi: `docs/ajan-skill-yol-haritasi.pdf` (çekirdek set + 4 haftalık kurulum).

## 9a. Arayüz dili ve kapsam kararları (21.11.2026)

- **Ortak tasarım dili (A11):** Kurumsal, işlev önce; güzellik yarışması değil. Casino personelinin alıştığı klasik
  masaüstü iş uygulaması düzeni korunur (menü, araç çubuğu, yoğun tablo/grid, form alanları, durum çubuğu, klavye
  kısayolları) ve modernleştirilir. Modernlik = net hiyerarşi, iyi tipografi, hizalama, okunur tablolar, sakin renk.
  **Oval/hap düğme, büyük yuvarlak kart ve süs animasyonu kullanılmaz** (düz ya da çok az köşe yumuşatma).
  Koyu tema seçeneği zorunlu; açık tema da var. Web, Windows ve mobil aynı tasarım token'larını kullanır.
- **Lisans açma/kapama (A4) ertelendi:** Proje sahibi tekrar söyleyene kadar plana alınmaz ve yapılmaz.

- **Ekran görselleri:** Proje sonunda (ve her aşama demosunda) tüm ekranların ürün hedefini gösteren örnek
  görselleri (Playwright ekran görüntüsü, sentetik veriyle) proje sahibine sunulur.
- **Araç kullanımı:** Kurulu skill ve ajanlar işin her adımında aktif kullanılır (spec/plan skill'leri, TDD,
  tasarım skill'leri, paralel worktree ajanları, `code-rules-reviewer`, `finans-denetci`, `veri-sagligi-denetci`).

## 9b. Modül kapsamı genişletmesi (23.11.2026, proje sahibi)

- **F&B:** Tam POS + stok: bar/restoran satış ekranı, adisyon/masa, comp ile ikram düşme, stok ve reçete takibi.
- **Muhasebe:** Tam muhasebe: hesap planı, yevmiye, mizan, bilanço; kasa, F&B ve otel hareketleri otomatik yevmiye kaydı üretir.
- **Inspector (masa denetçisi) arayüzü:** Ayrı, tablet uyumlu arayüz: oyuncu bahis takibi, masa chip sayımı ve fill/credit
  isteği, krupiye rotasyonu.
- **F&B yerleri:** Casino barı (salon içi, masaya/makineye servis) ve restoran (adisyonlu masa servisi). Oda servisi yok.
- **Muhasebe hesap planı:** Türkiye Tek Düzen Hesap Planı.
- **Inspector cihazı:** Hem tablet (mobil uygulamada ayrı arayüz) hem masa başı dokunmatik PC (Windows uygulamasında).
- **RFID cihazları:** Kart yazıcı ve okuyucu markasını proje sahibi iletecek; o gelene kadar cihaz bağımsız adaptör sözleşmesi.
- **RFID:** Oyuncu kartı RFID'li (resepsiyonda basım, girişte ve masada okutma). RFID chip yok; otel oda kartı ayrı kalır.

## 9c. Kapsam soru-cevabı (23.11.2026, proje sahibi; "karar" notları koordinatörün)

**Kasa**
- Ödeme: nakit (TRY/EUR/GBP/USD), kredi kartı (POS), banka havalesi, **kripto** (chip alımı ve kazanç ödemesi, aracı ödeme servisi üzerinden).
- Oyuncu hesabı (front money), chip emaneti, kasa kutusu (safe deposit) var.
- Belge: termal fiş, barkod/QR'lı fiş, büyük işlemde oyuncu imzası, e-posta/SMS dekont. Kasaya özel cihaz bağlantısı gerekmez.
- **Marker (kredi) verilir**, tahsilat takibi var.
- Dört göz: eşik üstü işlem, jackpot/el ödemesi, düzeltme/iptal, kasa devri.
- Oyun günü 24 saat değildir (gün kapanış saati ayarlanabilir).

**Canlı oyun / Inspector**
- Oyunlar: rulet, blackjack, baccarat/punto banco, Ultimate poker, Rus pokeri, Trio poker ve diğer casinoya karşı poker türleri.
- Oyuncu-oyuncu poker / turnuva: ayrı bir **turnuva modülü** olarak sonra.
- Rating: ortalama bahis + süre.
- Canlı takip: inspector her el sonu girdi/çıktı yapar; oyuncunun önündeki chip ve oynanan miktar anlık değişir.
- Inspector'da oyuncu bulma: isimden arama ve fotoğraflı koltuk düzeni.
- El sonu kaydı (karar): oyuncunun önündeki chip, bahis, kazanç/kayıp.
- Krupiye rotasyonu: pit boss belirler, rotayı yayınlar, anlık görünür.

**Slot**
- Ödeme: kartla kredi yükleme (cashless). Oyuncu takibi: kart takınca oturum.
- Bakım (karar): arıza kaydı + teknisyen ataması, sayaç okuma kontrolü.
- Promosyon: biriken puanın karşılığı para olarak kasadan oyuncu hesabına yüklenir.

**Oyuncu / PR / Loyalty**
- Girişte: kimlik okutma, yaş kontrolü, fotoğraf, kara liste uyarısı.
- Kara liste: casino listesi, devlet/yasal liste, kendi kendini kısıtlama.
- Loyalty: puanla seviye, seviye ayrıcalıkları, puan süresi, ödül kataloğu.
- Comp: yemek/içecek, otel odası, ulaşım/uçak, cashback.
- Pazarlama (SMS/e-posta kampanyası) gerekmez.
- Junket komisyon takibi ve grup gelişleri var.
- Host: oyuncu portföyü, ziyaret planı.
- Oyuncu servisleri: transfer, uçak bileti, vize/davet mektubu.

**F&B**
- Ödeme: comp ile ücretsiz (ve 9b'deki satış). Cihaz: mutfak yazıcısı/ekranı, bar ekranı.
- Stok: depo, reçete ile otomatik düşüm, sayım ve fire, satın alma/tedarikçi.
- Raporlar: ürün satış, comp maliyeti, stok değeri.

**Muhasebe**
- Cari, banka, demirbaş/amortisman, bütçe; kağıt fatura ve e-fatura.
- Personel: bordro ve puantaj. Kur: günlük değerleme, dönem sonu kur farkı, merkez bankası kuru otomatik + elle.
- Raporlar: mizan, bilanço, gelir tablosu, departman kârlılığı.

**Otel (S Vista):** Rezervasyon, resepsiyon, kat hizmetleri, online kanallar, folio, comp → oda, acente faturası,
depozito, SPA, toplantı salonu, çamaşırhane: hepsi.

**Güvenlik / uyum / yönetim**
- Kara para önleme modülü şimdilik yok.
- CCTV: işlem anında kamera işareti, kamera sistemine bağlantı, olay notu.
- Denetçi ekranı, günlük resmi rapor, vergi raporu, bağımsız denetim dökümü.
- Kullanıcı girişi: parmak izi/yüz ve kartla giriş. Personel izleme: hepsi.
- Yönetici paneli: canlı salon, günlük kâr/zarar, hedef/bütçe, mobil bildirim.
- Çoklu tesis: birden çok casino tek şirket, merkezi oyuncu kartı, merkezi raporlama.
- Dil: Türkçe ve İngilizce. Sunucu: casinoda kendi sunucusu.
- İnternet kesintisi olmaz varsayımı, ama önlem alınır (yedek hat, yerel sunucu, kesintide kasa için sınırlı mod).
- Modül sırası: koordinatör belirler (kolaydan zora).

## 9d. Eklenen fikirler (23.11.2026, proje sahibi onayı)

1. Oyuncu kaybı tahmini: ziyareti azalan değerli oyuncu için host'a uyarı.
2. Canlı salon ekranı (büyük TV): masa doluluğu, kazanç, açık masa.
3. Şüpheli davranış uyarısı (hile/iç işbirliği); **şartlar ve eşikler ayarlanabilir** olmalı.
4. Inspector için sesle giriş (masaya mikrofon takılabilir).
7. Akıllı comp önerisi: teorik kayba göre verilebilecek en fazla comp.
8. Krupiye performansı: el hızı, hata, masa sonucu.
9. Kesintide yerel mod: kasa sınırlı tutarla çalışır, sonra eşitler.
10. Sabah otomatik yönetim raporu (PDF, telefona).
Seçilmeyenler: oyuncu mobil uygulaması (5), QR menü (6).
14. Oyuncu tercih kartı (içki, masa, krupiye, sigara).
15. Chip envanter alarmı, chip yenileme/imha kaydı.
16. Masa sonucu sapma grafiği (gerçek vs beklenen).
18. Puan/comp dolandırıcılığı kontrolü (şişirilmiş rating, aynı kişiye sürekli comp).
19. Doğum günü/özel gün comp önerisi ve host hatırlatması.
20. Müdür için hızlı yanıt ekranı (oyuncu nerede, sonuç, comp).
25. Otomatik çapraz mutabakat (kasa ↔ drop ↔ muhasebe ↔ F&B ↔ folio), sabah fark listesi.
26. Veri sağlığı bekçisi: kapanmamış/eksik kayıtları bulur, sorumlusuna iş atar.
27. Ana veri onayı: kritik ayar değişikliği dört göz + önce/sonra kaydı.
29. İşlem yalnızca yetki + casino PC'sinden: **mobilden işlem yapılmaz**, casino bilgisayarı şarttır.
32. Anlık ekran güncellemesi.
33. IT kontrol paneli: sunucu/veritabanı/yedek sağlığı ve **bağlı cihaz takibi** (elektrik var mı, cihaz ayakta mı).
35. Olay defteri ve vardiya devir notu.

**Arayüz ilkesi (36 reddedildi):** Tek/genel arama kutusu yok. Program olay ve işlem bazlıdır: her işlem için ayrı ekran
ve ayrı pencere. Arama yalnızca ilgili ekranın kendi bağlamında yapılır (oyuncu ekranında oyuncu, İK'da personel),
tarih/takvim ile sınırlanır. Farklı alanların verisi tek listede karıştırılmaz.

## 10. Fizibilite raporunda güncellenmesi gerekenler

| Rapor ne diyor | Karar / güncelleme |
|---|---|
| Tek entegre platform (Core + Casino + PMS) | Ayrı satılan ürün ailesi: S Plus temel, S Slot ve S Vista isteğe bağlı modüller; lisansa göre modül açma/kapama |
| Genel Integration Hub, çok sayıda dış sistem | Rakip sistemlerden tek bağlantı NEXIO; slot verisi tek girişten (NEXIO adaptörü veya S Slot) |
| PMS için Opera/OHIP/IFC entegrasyon referansları | Opera/Elektra'ya bağlantı yok. Not: S Vista'nın kendi otel cihazları (kapı kilidi, POS, santral) ile bağlantısı ayrıca netleşmeli |
| GLI-13, PCI DSS, App Store gambling politikası, çok ülkeli regülasyon | Yalnızca KKTC; sertifika zorunluluğu varsayılmaz. Kart verisi işlenecekse PCI yine geçerli olabilir |
| RBAC/ABAC genel ifadesi | Ekran bazında görme + işlem yetkisi, yetki paneli, rol şablonları, oyuncu arama istisnası |
| Faz 5: cihaz entegrasyonu 6–12+ ay | S Slot için SMIB yolu kararı süreyi belirler; sertifika yoksa kısalabilir |
| Çok para birimi genel | TRY/EUR/GBP/USD zorunlu; TR/EN arayüz |
| Ekip ve bütçe uluslararası varsayımlarla | KKTC ölçeği ve agent destekli geliştirmeyle yeniden hesaplanmalı |

**Önerilen yeni sıra (öneri, karar değil):** (1) Ortak çekirdek + yetki paneli → (2) S Plus casino MVP + NEXIO
adaptörü → (3) Windows + mobil + web istemcileri → (4) S Vista → (5) S Slot.

## 11. Açık sorular

- [x] Kod kuralları listesindeki 2 numaralı madde nedir? → Kaldırıldı; yalnızca K1 ve K2 geçerli (26.10.2026).
- [x] S Slot: SMIB üreticisiyle anlaşma mı, kendi donanım mı? → **Hazır SMIB/arayüz kartı üreticisiyle anlaşılacak** (26.10.2026).
  Açık kalan: sahadaki makinelerde hangi protokoller var (SAS/G2S)?
- [x] NEXIO'ya bağlanmak için Advansys'ten resmi API alınabilir mi? → Proje sahibi bağlantının sorun olmayacağını
  belirtti (26.10.2026). Uygulama: önce Advansys'ten resmi API istenir; alınamazsa müşterinin kendi verisine
  meşru yollarla (müşterinin yetkisindeki rapor/dışa aktarım) erişilir. Bölüm 7 gereği NEXIO yazılımı
  decompile/tersine mühendislik ile açılmaz. Adaptör her iki yolu da aynı `ISlotDataSource` sözleşmesi arkasında saklar.
- [x] S Vista hangi otel cihazlarına bağlanacak? → **Yalnızca kapı kilidi / oda kartı basımı** (26.10.2026).
  POS, santral ve minibar bağlantısı yok. Açık kalan: hangi kapı kilidi markası/markaları?
- [x] Lisanslama ve fiyat modeli → **Modül + tesis başına**: her tesis için S Plus temel lisansı, S Slot ve S Vista ek lisans (26.10.2026).
- [x] İlk teslim edilecek ürün → **S Plus + NEXIO adaptörü** (26.10.2026). Pilot müşterinin adı henüz belirlenmedi.
- [ ] Teknik lider / entegrasyon mühendisi kim olacak? → Proje sahibi belli bir kişi olduğunu belirtti; ad/rol bekleniyor.
- [ ] Vergi ve resmi raporlama yükümlülükleri → Proje sahibi biliyor; ayrıntıları yazacak.
