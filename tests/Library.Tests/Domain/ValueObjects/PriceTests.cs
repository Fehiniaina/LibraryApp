// tests/Library.Tests/Domain/ValueObjects/PriceTests.cs
using Library.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Library.Tests.Domain.ValueObjects;

public class PriceTests
{
    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsArgumentException()
    {
        var act = () => new Price(-10, "EUR");
        act.Should().Throw<ArgumentException>().WithMessage("*négatif*");
    }

    [Fact]
    public void Constructor_WithInvalidCurrencyLength_ThrowsArgumentException()
    {
        var act = () => new Price(10, "EU");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var price1 = new Price(10, "EUR");
        var price2 = new Price(5, "EUR");

        var result = price1.Add(price2);

        result.Amount.Should().Be(15);
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ThrowsInvalidOperationException()
    {
        var price1 = new Price(10, "EUR");
        var price2 = new Price(5, "USD");

        var act = () => price1.Add(price2);

        act.Should().Throw<InvalidOperationException>();
    }
}