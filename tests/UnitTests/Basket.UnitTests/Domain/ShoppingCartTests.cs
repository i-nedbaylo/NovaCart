using FluentAssertions;
using NovaCart.Services.Basket.Domain.Entities;

namespace NovaCart.Tests.Basket.UnitTests.Domain;

public class ShoppingCartTests
{
    [Fact]
    public void Create_Should_ReturnCart_When_ValidBuyerId()
    {
        // Act
        var cart = ShoppingCart.Create("buyer-1");

        // Assert
        cart.Should().NotBeNull();
        cart.BuyerId.Should().Be("buyer-1");
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowArgumentException_When_InvalidBuyerId(string? buyerId)
    {
        // Act
        var act = () => ShoppingCart.Create(buyerId!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_Should_AddNewItem_When_ProductNotInCart()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, "Laptop", 999.99m, 1);

        // Assert
        cart.Items.Should().ContainSingle();
        cart.Items[0].ProductId.Should().Be(productId);
        cart.Items[0].ProductName.Should().Be("Laptop");
        cart.Items[0].Price.Should().Be(999.99m);
        cart.Items[0].Quantity.Should().Be(1);
    }

    [Fact]
    public void AddItem_Should_MergeQuantity_When_SameProductAdded()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, "Laptop", 999.99m, 1);
        cart.AddItem(productId, "Laptop", 999.99m, 2);

        // Assert
        cart.Items.Should().ContainSingle();
        cart.Items[0].Quantity.Should().Be(3);
    }

    [Fact]
    public void RemoveItem_Should_RemoveItem_When_ProductExists()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Laptop", 999.99m, 1);

        // Act
        cart.RemoveItem(productId);

        // Assert
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_Should_DoNothing_When_ProductNotFound()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);

        // Act
        cart.RemoveItem(Guid.NewGuid());

        // Assert
        cart.Items.Should().ContainSingle();
    }

    [Fact]
    public void UpdateItemQuantity_Should_UpdateQuantity_When_ProductExists()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Laptop", 999.99m, 1);

        // Act
        cart.UpdateItemQuantity(productId, 5);

        // Assert
        cart.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_Should_ThrowInvalidOperationException_When_ProductNotFound()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");

        // Act
        var act = () => cart.UpdateItemQuantity(Guid.NewGuid(), 5);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Clear_Should_RemoveAllItems()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        cart.AddItem(Guid.NewGuid(), "Mouse", 29.99m, 2);

        // Act
        cart.Clear();

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0);
    }

    [Fact]
    public void TotalPrice_Should_CalculateCorrectly_When_MultipleItems()
    {
        // Arrange
        var cart = ShoppingCart.Create("buyer-1");
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);
        cart.AddItem(Guid.NewGuid(), "Mouse", 29.99m, 2);

        // Act & Assert
        cart.TotalPrice.Should().Be(999.99m + 29.99m * 2);
    }
}
