using ExpenseTracker.Infrastructure.Persistence;
using Xunit;

namespace ExpenseTracker.Infrastructure.UnitTests.Persistence;

public sealed class MoneyToCentsConverterTests
{
    [Fact]
    public void ToCents_WithZero_ReturnsZero()
    {
        var cents = MoneyToCentsConverter.ToCents(0.00m);

        Assert.Equal(0L, cents);
    }

    [Fact]
    public void ToCents_WithPositiveAmount_ReturnsCents()
    {
        var cents = MoneyToCentsConverter.ToCents(10.25m);

        Assert.Equal(1025L, cents);
    }

    [Fact]
    public void ToCents_WithNegativeAmount_ReturnsSignedCents()
    {
        var cents = MoneyToCentsConverter.ToCents(-74.50m);

        Assert.Equal(-7450L, cents);
    }

    [Fact]
    public void ToCents_WithMaximumSupportedPositiveAmount_ReturnsLongMaxValue()
    {
        var cents = MoneyToCentsConverter.ToCents(92233720368547758.07m);

        Assert.Equal(long.MaxValue, cents);
    }

    [Fact]
    public void ToCents_AboveMaximumSupportedPositiveAmount_ThrowsOverflowException()
    {
        Action convertToCents = () =>
            MoneyToCentsConverter.ToCents(92233720368547758.08m);

        Assert.Throws<OverflowException>(convertToCents);
    }

    [Fact]
    public void ToCents_WithMinimumSupportedNegativeAmount_ReturnsLongMinValue()
    {
        var cents = MoneyToCentsConverter.ToCents(-92233720368547758.08m);

        Assert.Equal(long.MinValue, cents);
    }

    [Fact]
    public void ToCents_BelowMinimumSupportedNegativeAmount_ThrowsOverflowException()
    {
        Action convertToCents = () =>
            MoneyToCentsConverter.ToCents(-92233720368547758.09m);

        Assert.Throws<OverflowException>(convertToCents);
    }

    [Fact]
    public void FromCents_WithPositiveCents_ReturnsDecimal()
    {
        var amount = MoneyToCentsConverter.FromCents(1025L);

        Assert.Equal(10.25m, amount);
    }

    [Fact]
    public void FromCents_WithNegativeCents_ReturnsSignedDecimal()
    {
        var amount = MoneyToCentsConverter.FromCents(-7450L);

        Assert.Equal(-74.50m, amount);
    }

    [Fact]
    public void RoundTrip_WithRepresentativeValidAmounts_RetainsValues()
    {
        decimal[] amounts = [0.00m, 10.25m, -74.50m];

        foreach (var amount in amounts)
        {
            var cents = MoneyToCentsConverter.ToCents(amount);
            var convertedAmount = MoneyToCentsConverter.FromCents(cents);

            Assert.Equal(amount, convertedAmount);
        }
    }
}
