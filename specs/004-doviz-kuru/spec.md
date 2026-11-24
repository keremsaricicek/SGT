# Spec 004: Para birimleri ve döviz kuru

Modül: Configuration · Ürün: Çekirdek · Durum: Tamamlandı (26.10.2026)

## Amaç
Kasa ve raporlar dört para birimiyle çalışır. Kur, yetkili kişi tarafından girilir; geçmiş kurlar korunur,
böylece eski bir işlemin hangi kurla yapıldığı her zaman bulunur.

## Kabul kriterleri
- KK1: Desteklenen para birimleri TRY, EUR, GBP, USD; başka birim reddedilir. Ana para birimi TRY'dir.
- KK2: `admin.settings:exchange_rate` yetkilisi bir yabancı birim için alış ve satış kuru girer (ana birim karşılığı).
  Kurlar sıfırdan büyüktür, alış ≤ satış, en çok 6 ondalık basamak.
- KK3: Kur kayıtları değiştirilemez ve silinemez; yeni kur yeni kayıttır.
- KK4: Herhangi bir an için geçerli kur, o andan önce girilmiş en son kurdur. O ana kadar kur yoksa sonuç "kur yok"tur.
- KK5: Kur girişi denetime düşer (`ExchangeRateSetV1`, gerekçe kodu zorunlu).
- KK6: Diğer modüller kuru `Configuration.Contracts.IExchangeRateQuery` ile okur.
- KK7: Güncel kurlar listesi tüm oturum açmış kullanıcılara açıktır (kasa ekranı gösterir).
