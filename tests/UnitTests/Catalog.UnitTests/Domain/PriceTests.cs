using FluentAssertions;
using NovaCart.Services.Catalog.Domain.ValueObjects;

namespace NovaCart.Tests.Catalog.UnitTests.Domain;

public class PriceTests
{
    [Fact]
    public void Create_Should_ReturnPrice_When_ValidData()
    {
        // Act
        var price = Price.Create(29.99m, "USD");

        // Assert
        price.Amount.Should().Be(29.99m);
        price.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_Should_NormalizeCurrencyToUpperCase()
    {
        // Act
        var price = Price.Create(10m, "eur");

        // Assert
        price.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_NegativeAmount()
    {
        // Act
        var act = () => Price.Create(-1m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void Create_Should_AllowZeroAmount()
    {
        // Act
        var price = Price.Create(0m, "USD");

        // Assert
        price.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_EmptyCurrency()
    {
        // Act
        var act = () => Price.Create(10m, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency");
    }

    [Fact]
    public void Equality_Should_BeTrue_When_SameValues()
    {
        // Arrange
        var price1 = Price.Create(29.99m, "USD");
        var price2 = Price.Create(29.99m, "USD");

        // Assert
        price1.Should().Be(price2);
    }

    [Fact]
    public void Equality_Should_BeFalse_When_DifferentAmount()
    {
        // Arrange
        var price1 = Price.Create(29.99m, "USD");
        var price2 = Price.Create(19.99m, "USD");

        // Assert
        price1.Should().NotBe(price2);
    }

    [Fact]
    public void Equality_Should_BeFalse_When_DifferentCurrency()
    {
        // Arrange
        var price1 = Price.Create(29.99m, "USD");
        var price2 = Price.Create(29.99m, "EUR");

        // Assert
        price1.Should().NotBe(price2);
    }
}
