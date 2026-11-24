# BAŞLA — SGT Proje Paketi

Bu paket, SGT (Sentez Gaming Technologies) projesine başlamak için gereken her şeyi içerir.
Klasörün tamamı yeni proje deposunun köküne kopyalanır ve Claude Code bu klasörde açılır.

## İçindekiler

| Dosya | Ne |
|---|---|
| `docs/kararlar.md` | **En güncel kaynak.** Proje sahibiyle alınan tüm kararlar, fizibilite farkları, açık sorular |
| `docs/fizibilite-raporu.pdf` | İlk fizibilite raporu (19.10.2026). Eski; kararlarla çelişirse kararlar geçerli |
| `docs/ajan-skill-yol-haritasi.pdf` | Kullanılacak agent, skill, plugin ve MCP'ler; 4 haftalık kurulum planı |
| `docs/kod-kurallari.md` | K1 (SOLID/OOP) ve K2 (işlev ayrımı) kod kuralları, örnekler |
| `CLAUDE.md` | Her oturumda okunan kısa kural özeti |
| `.specify/memory/constitution.md` | Spec Kit proje anayasası (taslak) |
| `.claude/agents/code-rules-reviewer.md` | Kod kuralları denetçisi agent |
| `tests/Architecture/ArchitectureTests.cs`, `Directory.Build.props`, `build/BannedSymbols.txt` | Kuralları otomatik denetleyen dosyalar (kod aşamasında kullanılır; henüz derlenmedi) |

## Okuma sırası (yapay zeka için)

1. `CLAUDE.md`
2. `docs/kararlar.md`
3. `.specify/memory/constitution.md`
4. `docs/fizibilite-raporu.pdf`
5. `docs/ajan-skill-yol-haritasi.pdf`
6. `docs/kod-kurallari.md` (kod aşamasında)

## İlk mesaj olarak yapıştırılacak metin

```
Bu depo SGT projesinin başlangıç paketi. BASLA.md'deki okuma sırasıyla tüm dosyaları oku.

Şu an Faz 0'dayız (planlama). Kod yazma, proje iskeleti kurma.
Ben kod bilmiyorum; bana sade Türkçe ile anlat, teknik terimleri açıkla.

Senden istediklerim, sırayla ve her adımda onayımı alarak:
1. Okuduklarını 10 maddede özetle ve anlamadığın veya çelişkili gördüğün noktaları listele.
2. docs/kararlar.md'deki "Açık sorular"ı benimle tek tek netleştir.
3. Fizibilite raporunu kararlar.md'ye göre güncelleyen "Fizibilite v2" taslağını hazırla
   (ürün paketleri, NEXIO bağlantısı, S Slot donanım kararı, KKTC, yetki modeli, yeni faz sırası, ekip ve bütçe).
4. Domain sözlüğü (Türkçe/İngilizce terimler) ve S Plus'ın ekran listesini çıkar.
5. Bunlar onaylanınca ajan-skill-yol-haritasi.pdf'teki "Hafta 1" kurulumuna geçelim.
```

## Hatırlatmalar

- Anayasa taslaktır; proje sahibi onaylayınca "Durum" satırı güncellenir.
- Gerçek oyuncu/misafir verisi hiçbir zaman yapay zekaya verilmez.
- Kodlama başladığında deneyimli bir teknik lider (insan) para/kasa akışlarını onaylamalıdır.
