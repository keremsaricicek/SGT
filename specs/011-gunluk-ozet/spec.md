# Spec 011: Günlük casino özeti (R1)

Modül: Reporting (+ Finance, GamingFloor, Loyalty sözleşmeleri) · Ürün: S Plus · Durum: Tamamlandı (26.10.2026)
Ekran: report.daily

## Amaç
Yönetici telefondan veya web panelinden günün durumunu tek ekranda görür. Rapor modülü hiçbir modülün
tablosuna erişmez; her modül kendi günlük özetini Contracts sorgusuyla verir (K2).

## Kabul kriterleri
- KK1: İş günü KKTC saatiyle 00:00–24:00'tır (oyun günü başlangıç saati ileride ayarlanabilir olacak).
- KK2: Kasa: para birimi bazında çip satışı, çip bozdurma, döviz alım/satım (yabancı tutar), ters kayıt sayısı;
  açık ve kapanan vardiya sayısı; kapanış farklarının toplamı.
- KK3: Canlı oyun: biten rating sayısı, para birimi bazında toplam teorik kazanç ve casino sonucu (= −oyuncu sonucu).
- KK4: Slot: para birimi bazında toplam giren ve makine kazancı (okuma farklarından).
- KK5: Loyalty: kazanılan ve harcanan toplam puan.
- KK6: Verisi olmayan bölüm boş döner (lisanssız ürünün verisi oluşmaz); rapor `report.daily:view` yetkisi ister ve tenant kapsamlıdır.
- KK7: Tarih verilmezse bugün; geçmiş bir gün de sorgulanabilir.
