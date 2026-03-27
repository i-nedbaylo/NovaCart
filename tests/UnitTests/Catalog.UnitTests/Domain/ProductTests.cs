using FluentAssertions;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Domain.Events;
using NovaCart.Services.Catalog.Domain.ValueObjects;

namespace NovaCart.Tests.Catalog.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_Should_ReturnProduct_When_ValidData()
    {
        // Arrange
        var name = "Test Product";
        var description = "A test product";
        var slug = "test-product";
        var priceAmount = 29.99m;
        var currency = "USD";
        var categoryId = Guid.NewGuid();
        var imageUrl = "https://example.com/image.png";

        // Act
        var product = Product.Create(name, description, slug, priceAmount, currency, categoryId, imageUrl);

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Slug.Should().Be(slug);
        product.Price.Amount.Should().Be(priceAmount);
        product.Price.Currency.Should().Be("USD");
        product.CategoryId.Should().Be(categoryId);
        product.ImageUrl.Should().Be(imageUrl);
        product.Status.Should().Be(ProductStatus.Active);
        product.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_Should_RaiseProductCreatedDomainEvent_When_Created()
    {
        // Act
        var product = Product.Create("Test", "Desc", "test", 10m, "USD", Guid.NewGuid());

        // Assert
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedDomainEvent>();
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_NegativePrice()
    {
        // Act
        var act = () => Product.Create("Test", "Desc", "test", -5m, "USD", Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_ThrowArgumentException_When_EmptyName()
    {
        // Act
        var act = () => Product.Create("", "Desc", "test", 10m, "USD", Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Should_NormalizeSlugToLowerCase_When_UpperCaseSlug()
    {
        // Act
        var product = Product.Create("Test", "Desc", "Test-Slug", 10m, "USD", Guid.NewGuid());

        // Assert
        product.Slug.Should().Be("test-slug");
    }

    [Fact]
    public void Update_Should_UpdateProperties_When_ValidData()
    {
        // Arrange
        var product = Product.Create("Old Name", "Old Desc", "old-slug", 10m, "USD", Guid.NewGuid());
        var newCategoryId = Guid.NewGuid();

        // Act
        product.Update("New Name", "New Desc", "new-slug", 20m, "EUR", newCategoryId, "https://new.com/img.png");

        // Assert
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New Desc");
        product.Slug.Should().Be("new-slug");
        product.Price.Amount.Should().Be(20m);
        product.Price.Currency.Should().Be("EUR");
        product.CategoryId.Should().Be(newCategoryId);
        product.ImageUrl.Should().Be("https://new.com/img.png");
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Discontinue_Should_SetStatusToDiscontinued()
    {
        // Arrange
        var product = Product.Create("Test", "Desc", "test", 10m, "USD", Guid.NewGuid());

        // Act
        product.Discontinue();

        // Assert
        product.Status.Should().Be(ProductStatus.Discontinued);
    }
}
