// tests/Library.Tests/Domain/ValueObjects/PriceTests.cs
using FluentAssertions;

using Library.Domain.ValueObjects;

namespace Library.Tests.Domain.ValueObjects;

public class PriceTests
{
    [Fact]
    public void ConstructorWithNegativeAmountThrowsArgumentException()
    {
        var act = () => new Price(-10, "EUR");
        act.Should().Throw<ArgumentException>().WithMessage("*négatif*");
    }

    [Fact]
    public void ConstructorWithInvalidCurrencyLengthThrowsArgumentException()
    {
        var act = () => new Price(10, "EU");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddWithSameCurrencyReturnsSum()
    {
        var price1 = new Price(10, "EUR");
        var price2 = new Price(5, "EUR");

        var result = price1.Add(price2);

        result.Amount.Should().Be(15);
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public void AddWithDifferentCurrencyThrowsInvalidOperationException()
    {
        var price1 = new Price(10, "EUR");
        var price2 = new Price(5, "USD");

        var act = () => price1.Add(price2);

        act.Should().Throw<InvalidOperationException>();
    }
}