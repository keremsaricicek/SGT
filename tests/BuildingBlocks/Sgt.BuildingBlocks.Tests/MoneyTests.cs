using Sgt.BuildingBlocks;

namespace Sgt.BuildingBlocks.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Of_normalizes_currency_to_upper_case()
    {
        var money = Money.Of(10m, "eur");

        Assert.Equal("EUR", money.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Of_rejects_invalid_currency(string currency)
    {
        Assert.Throws<ArgumentException>(() => Money.Of(10m, currency));
    }

    [Fact]
    public void Add_sums_amounts_in_same_currency()
    {
        var total = Money.Of(10.25m, "TRY").Add(Money.Of(4.75m, "TRY"));

        Assert.Equal(Money.Of(15m, "TRY"), total);
    }

    [Fact]
    public void Add_rejects_different_currencies()
    {
        Assert.Throws<InvalidOperationException>(() => Money.Of(10m, "TRY").Add(Money.Of(1m, "USD")));
    }

    [Fact]
    public void Negate_reverses_sign_and_keeps_currency()
    {
        Assert.Equal(Money.Of(-5m, "GBP"), Money.Of(5m, "GBP").Negate());
    }

    [Fact]
    public void Equal_amounts_and_currencies_are_equal()
    {
        Assert.Equal(Money.Of(1m, "USD"), Money.Of(1.00m, "usd"));
    }
}
