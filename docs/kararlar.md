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
- Ayrıntı: `docs/kod-kurallari.md`. Proje sahibinin listesinde 2 numaralı madde eksik; geldiğinde K3 olarak eklenecek.

## 9. Diğer

- **Merit Event Studio** ayrı bir ürün ve ayrı bir depo olarak kalır; aynı kural paketi orada da kullanılabilir.
- Agent ve skill seçimi: `docs/ajan-skill-yol-haritasi.pdf` (çekirdek set + 4 haftalık kurulum).

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

- [ ] Kod kuralları listesindeki 2 numaralı madde nedir?
- [ ] S Slot: SMIB üreticisiyle anlaşma mı, kendi donanım mı? Sahadaki makinelerde hangi protokoller var (SAS/G2S)?
- [ ] NEXIO'ya bağlanmak için Advansys'ten resmi API (API Library) ve erişim alınabilir mi?
- [ ] S Vista hangi otel cihazlarına bağlanacak (kapı kilidi, POS, santral, minibar)?
- [ ] Lisanslama ve fiyat modeli (modül başına, tesis başına, kullanıcı başına?).
- [ ] İlk pilot müşteri ve ilk teslim edilecek ürün hangisi?
- [ ] Teknik lider / entegrasyon mühendisi kim olacak?
- [ ] Vergi ve resmi raporlama yükümlülükleri (mali müşavir teyidi).
