# SGT — Fizibilite v2 (Taslak)

Tarih: 26.10.2026 · Kaynak: `docs/kararlar.md` + ilk fizibilite (19.10.2026). Çelişkide `kararlar.md` geçerlidir.
Durum: **Taslak, proje sahibi onayı bekliyor.**

## 1. Özet

| Konu | v1 (eski) | v2 |
|---|---|---|
| Ürün yapısı | Tek entegre platform | Ayrı satılan aile: S Plus (temel), S Slot, S Vista (opsiyonel) |
| Pazar | Çok ülke | Yalnızca KKTC; TRY/EUR/GBP/USD, TR/EN |
| Dış bağlantı | Genel Integration Hub, çok vendor | Tek dış bağlantı NEXIO (opsiyonel) |
| Otel entegrasyonu | Opera/OHIP/IFC | Yok. S Vista yalnızca kapı kilidine bağlanır |
| Sertifika | GLI-13, PCI DSS | Zorunlu varsayılmaz; kart verisi işlenirse PCI değerlendirilir |
| Yetki | Genel RBAC/ABAC | Ekran bazlı görme + işlem yetkisi, yetki paneli, rol şablonları |
| Lisans | Belirsiz | Modül + tesis başına |
| İlk teslim | Otel PMS + CRM + casino | S Plus + NEXIO adaptörü |

Teknik yapılabilirlik: **yüksek.** Kapsam daraldığı için (tek pazar, tek dış bağlantı, sertifika yok) v1 süreleri kısalır.

## 2. Ürünler ve modüller

| Ürün | Modüller (K2 bounded context) |
|---|---|
| Ortak çekirdek (görünmez) | Identity, Yetki paneli, Audit, Konfigürasyon, Lisans, Integration |
| S Plus | Party, CasinoPlayer, GamingFloor (canlı oyun + slot ekranları), Finance (kasa), Loyalty, Workflow, Raporlar |
| S Slot | Slot veri toplama (SMIB üzerinden SAS/G2S) |
| S Vista | Hotel (rezervasyon, resepsiyon, folio, housekeeping) + kapı kilidi adaptörü |

Lisans modülü, satın alınmayan ürünü kapatır; S Plus tek başına çalışır.
Slot ekranları veriyi tek sözleşmeden (`ISlotDataSource`) alır: NEXIO adaptörü, S Slot veya manuel giriş.

## 3. Entegrasyonlar

| Bağlantı | Yöntem | Risk |
|---|---|---|
| NEXIO | Önce Advansys resmi API; olmazsa müşterinin kendi rapor/dışa aktarımı. Decompile yok | Orta: API erişimi belirsiz |
| S Slot ↔ makine | Hazır SMIB üreticisiyle anlaşma; SAS (lisanslı) veya G2S | Yüksek: üretici anlaşması ve sahadaki protokol bilinmiyor |
| S Vista ↔ kapı kilidi | Marka bazlı adaptör | Düşük–orta: marka bilinmiyor |

## 4. Faz sırası

| Faz | İçerik | Tahmini süre* |
|---|---|---|
| 0 | Planlama: kararlar, fizibilite v2, sözlük, ekran listesi, anayasa onayı, araç kurulumu | 3–4 hafta |
| 1 | Ortak çekirdek: kimlik, yetki paneli, audit, lisans, konfigürasyon | 2–3 ay |
| 2 | S Plus casino MVP: oyuncu, kasa, canlı oyun, loyalty, slot ekranları + NEXIO adaptörü | 4–6 ay |
| 3 | İstemciler: Windows (operasyon), mobil, web paneli (Faz 1–2 ile paralel başlar) | Faz 2 ile örtüşür |
| 4 | S Vista + kapı kilidi | 4–6 ay |
| 5 | S Slot (SMIB anlaşmasına bağlı) | 4–8 ay |

\* Agent destekli geliştirme ve küçük ekip varsayımıyla kaba tahmin; Faz 1 sonunda gerçek hızla yeniden hesaplanır.
İlk satılabilir sürüm (Faz 1+2+temel istemciler): **~8–10 ay.**

## 5. Ekip (minimum)

| Rol | Kişi | Not |
|---|---|---|
| Proje sahibi / ürün | 1 | Kararlar, öncelik, saha bilgisi |
| Teknik lider (.NET, finans akışı onayı) | 1 | Belirlendi, ad bekleniyor. Para akışı PR'ları onun onayıyla birleşir |
| Full-stack geliştirici | 1–2 | Agent'larla birlikte çalışır |
| Entegrasyon/saha mühendisi | 1 (Faz 2'den) | NEXIO, sonra kapı kilidi ve SMIB |
| Test/saha destek | 1 (pilotta) | Pilot casinoda kabul testi |

## 6. Bütçe kalemleri

Tutarlar KKTC maaş düzeyi netleşince doldurulacak.

| Kalem | Tür |
|---|---|
| Personel (bölüm 5) | Aylık |
| Claude/AI araç abonelikleri | Aylık |
| Bulut / sunucu (test + pilot) | Aylık |
| SMIB kartları ve test makinesi | Faz 5, tek seferlik |
| Kapı kilidi test donanımı | Faz 4, tek seferlik |
| Bağımsız güvenlik testi (pentest) | Pilot öncesi, tek seferlik |
| Hukuk/mali müşavir, marka tescili | Tek seferlik |

## 7. Başlıca riskler

1. SMIB üreticisi anlaşması ve sahadaki protokol (S Slot'u belirler).
2. NEXIO veri erişimi (ilk ürünü belirler).
3. Para akışlarında hata: append-only kayıt, maker-checker, insan onayı ile azaltılır.
4. AI kodunun AI tarafından incelenmesi: finans kodunda insan uzman zorunlu.
5. Vergi/resmi raporlama gereksinimleri henüz yazılı değil.

## 8. Açık kalanlar

Teknik lider adı · vergi/raporlama ayrıntıları · sahadaki slot protokolü · kapı kilidi markası · pilot müşteri adı.
