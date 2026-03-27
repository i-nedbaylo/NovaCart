using FluentAssertions;
using NSubstitute;
using NovaCart.Services.Catalog.Application.Products.Queries;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Tests.Catalog.UnitTests.Application;

public class GetProductByIdHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _handler = new GetProductByIdHandler(_productRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnProductDto_When_ProductExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", "Description", "test-product", 29.99m, "USD", categoryId);
        var query = new GetProductByIdQuery(product.Id);

        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Test Product");
        result.Value.PriceAmount.Should().Be(29.99m);
        result.Value.PriceCurrency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_ProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("NotFound");
    }
}
