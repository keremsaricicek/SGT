# S Plus Windows istemcisi (operasyon)

Kasiyer, pit ve oyuncu ekranları için Windows masaüstü uygulaması (WPF, MVVM).
Veritabanına bağlanmaz; her işlem kimliği doğrulanmış API (`/api/v1`) üzerinden yapılır.

## Klasörler

| Proje | Ne yapar |
|---|---|
| `Sgt.Windows.Core` | Ekranların mantığı (ViewModel), API istemcisi, giriş (OIDC). WPF'e bağlı değildir, Linux'ta da test edilir. |
| `Sgt.Windows.App` | Yalnızca görünüm: XAML pencereleri, pencere servisi, ayarlar. İş mantığı yoktur. |
| `Sgt.Windows.Core.Tests` | ViewModel, servis ve API sözleşmesi testleri (xUnit). |

`Sgt.Windows.Core/Features` altında her özellik kendi klasöründedir:

- `Login` — giriş (Y1). Şifre ve doğrulama kodu uygulamaya yazılmaz; sistem tarayıcısında Keycloak sayfası açılır (OIDC + PKCE, ADR-0003).
- `Shell` — ana pencere (Y2). Menü sunucudan gelir (`/api/v1/identity/me/menu`); kullanıcı yalnızca yetkili olduğu ekranları görür.
  Her ekran ayrı pencerede açılır; açık ekran tekrar seçilince mevcut pencere öne gelir.
- `Cage` — vardiya aç/bağlan/kapat (sayım), çip satış/bozdurma, döviz (iki yön), kasa özeti, düzeltme talebi.
- `Players` — oyuncu arama (en az 2 karakter) ve oyuncu kartı (salt okunur, belge numarası maskeli).
- `Floor` — masalar, masa aç/kapat, oyuncu rating başlat/bitir.

## Önemli davranışlar

- **Çift para hareketi olmaz:** Her kasa işlemi bir kez işlem numarası (TransactionId) alır. Ağ hatası veya
  sunucu hatasından sonra aynı bilgilerle tekrar "Onayla" denirse aynı numara gönderilir; sunucu ikinci hareket yazmaz.
  Tutar, oyuncu veya tür değişirse yeni işlem sayılır.
- **Her istek** `X-Terminal` (bilgisayar adı) ve `X-Correlation-Id` (işlem başına iz numarası) başlığı taşır; audit kaydı bunları kullanır.
- **Hatalar** sunucunun gönderdiği açıklamayla ve hata koduyla (ör. `Finance.VarianceNoteRequired`) ekranda gösterilir.
- **Vardiya kapanışı:** sayımda fark varsa açıklama yazılmadan kapanış yapılamaz.
- **Yetki:** butonlar kullanıcının yetkisine göre açılır/kapanır. Bu yalnızca kolaylıktır; asıl kontrol API'dedir.
- **Token** yalnızca bellekte tutulur; diske veya loga yazılmaz. Çıkışta Keycloak oturumu da kapatılır (ortak terminal).

## Klavye

| Tuş | İşlev |
|---|---|
| Enter / çift tık | Menüde seçili ekranı aç; arama kutusunda ara |
| Ctrl+F | Menüde ara |
| Ctrl+Enter | Kasa işlemini / düzeltme talebini onayla; oyuncu kartını aç |
| F5 | Ekranı yenile |
| Esc | Ekran penceresini kapat |
| Alt + altı çizili harf | İlgili alana / butona git |

## Ayarlar (`Sgt.Windows.App/appsettings.json`)

| Ayar | Açıklama |
|---|---|
| `Api:BaseAddress` | API adresi, ör. `https://sgt-api.ornek-casino.local/` |
| `Authentication:Authority` | Keycloak realm adresi, ör. `https://giris.ornek-casino.local/realms/sgt` |
| `Authentication:ClientId` | Keycloak istemcisi: `sgt-windows` (public, Authorization Code + PKCE) |
| `Authentication:LoopbackPort` | Tarayıcı dönüş portu; `0` = boş port seçilir |
| `Authentication:RequireHttps` | Üretimde `true`. Yalnızca yerel geliştirmede `false` |
| `Ui:Language` | `tr` (varsayılan) veya `en` |

Yerel geliştirme için `DOTNET_ENVIRONMENT=Development` ayarlanırsa `appsettings.Development.json`
(API `http://localhost:5067/`, Keycloak `http://localhost:8080/realms/sgt`) kullanılır.
Ayar dosyalarına secret veya veritabanı bilgisi yazılmaz.

## Komutlar

```bash
dotnet build clients/windows/Sgt.Windows.slnx            # derleme (uyarı = hata; Linux'ta da çalışır)
dotnet test  clients/windows/Sgt.Windows.Core.Tests      # testler
dotnet run --project clients/windows/Sgt.Windows.App     # uygulama (yalnızca Windows)
```

GitHub Actions: `.github/workflows/windows.yml` (windows-latest üzerinde derleme + test + paket).

## API sözleşmesi

İstek/yanıt tipleri `docs/api/openapi-v1.json` ile birebir aynıdır. Kod üretici yerine elle yazılmış
kayıtlar kullanılır; `Contract/OpenApiContractTests` her rotanın ve her `[ApiSchema]` işaretli tipin
alanlarının sözleşmeyle aynı olduğunu her test çalışmasında denetler. Sözleşme değişirse test kırmızı olur.

## Metinler (TR/EN)

`Sgt.Windows.Core/Localization/Strings.resx` (Türkçe) ve `Strings.en.resx` (İngilizce). Yeni metin iki dosyaya da eklenir.
