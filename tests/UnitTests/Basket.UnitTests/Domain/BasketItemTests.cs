using FluentAssertions;
using NovaCart.Services.Basket.Domain.Entities;

namespace NovaCart.Tests.Basket.UnitTests.Domain;

public class BasketItemTests
{
    [Fact]
    public void Create_Should_ReturnItem_When_ValidData()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = BasketItem.Create(productId, "Laptop", 999.99m, 2);

        // Assert
        item.Should().NotBeNull();
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Laptop");
        item.Price.Should().Be(999.99m);
        item.Quantity.Should().Be(2);
        item.TotalPrice.Should().Be(999.99m * 2);
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_EmptyProductId()
    {
        // Act
        var act = () => BasketItem.Create(Guid.Empty, "Laptop", 999.99m, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("productId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowArgumentException_When_InvalidProductName(string? productName)
    {
        // Act
        var act = () => BasketItem.Create(Guid.NewGuid(), productName!, 10m, 1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_NegativePrice()
    {
        // Act
        var act = () => BasketItem.Create(Guid.NewGuid(), "Laptop", -1m, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("price");
    }

    [Fact]
    public void Create_Should_AllowZeroPrice()
    {
        // Act
        var item = BasketItem.Create(Guid.NewGuid(), "Free Item", 0m, 1);

        // Assert
        item.Price.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_Should_ThrowArgumentException_When_InvalidQuantity(int quantity)
    {
        // Act
        var act = () => BasketItem.Create(Guid.NewGuid(), "Laptop", 999.99m, quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public void UpdateQuantity_Should_UpdateValue_When_ValidQuantity()
    {
        // Arrange
        var item = BasketItem.Create(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act
        item.UpdateQuantity(5);

        // Assert
        item.Quantity.Should().Be(5);
        item.TotalPrice.Should().Be(999.99m * 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateQuantity_Should_ThrowArgumentException_When_InvalidQuantity(int quantity)
    {
        // Arrange
        var item = BasketItem.Create(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act
        var act = () => item.UpdateQuantity(quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("newQuantity");
    }

    [Fact]
    public void UpdatePrice_Should_UpdateValue_When_ValidPrice()
    {
        // Arrange
        var item = BasketItem.Create(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act
        item.UpdatePrice(1099.99m);

        // Assert
        item.Price.Should().Be(1099.99m);
    }

    [Fact]
    public void UpdatePrice_Should_ThrowArgumentException_When_NegativePrice()
    {
        // Arrange
        var item = BasketItem.Create(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act
        var act = () => item.UpdatePrice(-1m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("newPrice");
    }
}
