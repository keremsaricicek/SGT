# Quickstart: Demo ortamı doğrulama

1. Yerel: `build/demo.sh up` → çıktıda web/API/Keycloak adresleri ve demo kullanıcıları.
2. `curl localhost:5080/health` → `ok`.
3. Casino müdürü demo kullanıcısıyla giriş → günlük özet (son 30 günün her günü dolu).
4. Oyuncu arama: 500 oyuncu; birkaçı kısıtlı.
5. `build/demo.sh up` ikinci kez → veri çoğalmaz.
6. Otomatik: `dotnet test tests/Api/Sgt.Api.Tests --filter "FullyQualifiedName~DemoData"` (küçük ölçek, yerel PostgreSQL).
