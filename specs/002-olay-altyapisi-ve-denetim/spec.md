# Spec 002: Olay altyapısı ve merkezi denetim kaydı

Modül: BuildingBlocks + Audit · Ürün: Çekirdek · Durum: Onaylandı (koordinatör, 26.10.2026)
Kaynak: anayasa II, III, VII; ADR-0004.

## Amaç
Modüller birbirine yalnızca olaylarla haber verir; olay kaybolmaz, iki kez işlenmez, hata olursa operatör görür.
Tüm kritik işlemler tek bir denetim kaydında aranabilir.

## Kabul kriterleri
- KK1: Olay, iş verisiyle aynı transaction'da outbox'a yazılır; iş kaydı yazılmazsa olay da yoktur.
- KK2: Dağıtıcı bekleyen olayları abonelere iletir ve işlendi olarak işaretler.
- KK3: Aynı olay bir tüketiciye ikinci kez gelirse ikinci etki oluşmaz (inbox).
- KK4: Hata alan olay üstel beklemeyle yeniden denenir; 10. başarısız denemede dead-letter olur ve hata mesajı saklanır.
- KK5: Operatör (`admin.integrations:view`) dead-letter listesini görür; `admin.integrations:reprocess` ile yeniden işletir.
- KK6: Denetime tabi her olay (aktör, terminal, gerekçe, correlation id taşıyan) Audit modülüne bir kayıt olarak yazılır.
- KK7: Denetim kayıtları değiştirilemez ve silinemez.
- KK8: `admin.audit:view` yetkilisi denetim kayıtlarını tarih aralığı, aktör ve correlation id ile arar; sonuçlar tenant kapsamlıdır.
- KK9: Yetki paneli değişiklikleri (Spec 001) `PermissionChangedV1` olayı olarak yayınlanır ve denetime düşer.

## Kapsam dışı
Harici mesaj kuyruğu (ADR-0004), denetim kaydının dışa aktarımı.
