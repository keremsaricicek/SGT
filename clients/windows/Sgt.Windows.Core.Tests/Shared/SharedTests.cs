using System.Globalization;
using Sgt.Windows.Core.Api;
using Sgt.Windows.Core.Shared;

namespace Sgt.Windows.Core.Tests.Shared;

public sealed class SharedTests
{
    [Fact]
    public void Ayni_girdi_ayni_islem_kimligi_farkli_girdi_yeni_kimlik()
    {
        var pending = new PendingOperation(new SequentialIds());

        var first = pending.For(("A", 100m));
        Assert.Same(first, pending.For(("A", 100m)));

        var changed = pending.For(("A", 200m));
        Assert.NotEqual(first.TransactionId, changed.TransactionId);

        pending.Complete();
        Assert.Null(pending.Current);
        Assert.NotEqual(changed.TransactionId, pending.For(("A", 200m)).TransactionId);
    }

    [Theory]
    [InlineData("tr-TR", "1.250,50", 1250.50)]
    [InlineData("en-GB", "1,250.50", 1250.50)]
    [InlineData("tr-TR", "100", 100)]
    public void Tutar_kulture_gore_cozulur(string culture, string text, decimal expected)
    {
        var money = Money.ParsePositive(text, Currency.Try, CultureInfo.GetCultureInfo(culture));

        Assert.Equal(new Money(expected, Currency.Try), money);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("")]
    [InlineData("on")]
    public void Pozitif_olmayan_veya_gecersiz_tutar_kabul_edilmez(string text) =>
        Assert.Null(Money.ParsePositive(text, Currency.Try, CultureInfo.InvariantCulture));

    [Fact]
    public void Yalnizca_desteklenen_para_birimleri_vardir()
    {
        Assert.Equal(["TRY", "EUR", "GBP", "USD"], Currency.All.Select(c => c.Code));
        Assert.Equal(Currency.Usd, Currency.Find(" usd "));
        Assert.Null(Currency.Find("JPY"));
    }

    [Fact]
    public void Rota_parametreleri_doldurulur_ve_eksik_parametre_hata_verir()
    {
        var id = Guid.Parse("77777777-7777-7777-7777-777777777777");

        Assert.Equal(
            "api/v1/cage/shifts/77777777-7777-7777-7777-777777777777/chip-sales",
            ApiRoutes.ChipSales.Build(id).ToString());
        Assert.Throws<ArgumentException>(() => ApiRoutes.ChipSales.Build());
    }
}
