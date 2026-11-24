# S Plus — Ekran Listesi (Taslak)

Her ekranın yetki panelinde **Görebilir (G)** ve **İşlem (İ)** satırı vardır; "İşlem alt yetkileri" ayrıca verilir.
İstemci: W = Windows, M = Mobil, Web = web paneli.

## Çekirdek / Yönetim

| # | Ekran | İşlem alt yetkileri | İstemci |
|---|---|---|---|
| Y1 | Giriş / şifre / MFA | — | W M Web |
| Y2 | Ana menü (yetkiye göre) | — | W M Web |
| Y3 | Kullanıcılar | oluştur, pasifleştir | W Web |
| Y4 | Roller ve yetki paneli | rol oluştur, yetki ver/al (yalnızca süper admin) | Web |
| Y5 | Yetki değişiklik geçmişi | — (salt okunur) | Web |
| Y6 | Audit kayıtları | — (salt okunur) | Web |
| Y7 | Tesis ve konfigürasyon (para birimi, kur, dil) | kur girişi | Web |
| Y8 | Lisans ve modüller | — (salt okunur) | Web |
| Y9 | Entegrasyon durumu / hata kuyruğu (NEXIO) | tekrar işle | Web |

## Oyuncu

| # | Ekran | İşlem alt yetkileri | İstemci |
|---|---|---|---|
| O1 | Oyuncu arama | — | W M |
| O2 | Oyuncu "şu an nerede" (ortak arama istisnası) | — | W M |
| O3 | Oyuncu kartı / profil | oluştur, düzenle, kart bas, kart iptal | W |
| O4 | Oyuncu geçmişi (ziyaret, oyun, kasa) | — | W M |
| O5 | Kara liste / kısıtlama | ekle, kaldır (dört göz) | W |

## Canlı oyun

| # | Ekran | İşlem alt yetkileri | İstemci |
|---|---|---|---|
| C1 | Masa haritası / pit görünümü | — | W M |
| C2 | Masa aç / kapat | aç, kapat | W |
| C3 | Oyuncu derecelendirme (rating) | başlat, bitir, düzelt | W |
| C4 | Masa float ve çip hareketi (fill/credit) | talep, onay | W |
| C5 | Drop / masa sonucu | gir, onayla | W |

## Slot

| # | Ekran | İşlem alt yetkileri | İstemci |
|---|---|---|---|
| S1 | Slot salon haritası / makine durumu | — | W M |
| S2 | Makine detayı (sayaçlar, olaylar) | — | W |
| S3 | Jackpot / el ödemesi | kaydet, onayla (dört göz) | W |
| S4 | Manuel slot verisi girişi / import | gir, import et | W Web |
| S5 | Makine tanımları ve mapping | tanımla | Web |

## Kasa (Cage)

| # | Ekran | İşlem alt yetkileri | İstemci |
|---|---|---|---|
| K1 | Kasiyer vardiyası aç / kapat | aç, kapat, sayım | W |
| K2 | Para giriş / çıkış (oyuncu) | giriş, çıkış | W |
| K3 | Çip satış / bozdurma | sat, boz | W |
| K4 | Döviz işlemleri | işlem | W |
| K5 | Düzeltme / ters kayıt | talep, onayla (dört göz) | W |
| K6 | Kasa özeti ve mutabakat | — | W Web |

## Loyalty / Host

| # | Ekran | İşlem alt yetkileri | İstemci |
|---|---|---|---|
| L1 | Puan ve seviye | puan düzelt (dört göz) | W M |
| L2 | Comp talebi / onayı | talep, onayla | W M |
| L3 | Host portföyü (oyuncularım) | — | M |
| L4 | Loyalty kuralları | tanımla | Web |

## Raporlar

| # | Ekran | İstemci |
|---|---|---|
| R1 | Günlük casino özeti (dashboard) | M Web |
| R2 | Masa / pit raporları | W Web |
| R3 | Slot raporları | W Web |
| R4 | Kasa ve mutabakat raporları | W Web |
| R5 | Oyuncu raporları | Web |
| R6 | Resmi/vergi raporları (gereksinim bekleniyor) | Web |

Toplam: 40 ekran. S Vista ve S Slot ekranları ilgili fazlarda ayrıca listelenecek.
