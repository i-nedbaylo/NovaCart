using FluentAssertions;
using NSubstitute;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Application.Products.Commands;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Domain.Repositories;

namespace NovaCart.Tests.Catalog.UnitTests.Application;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateProductHandler(_productRepository, _categoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessWithId_When_ValidCommand()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            "Test Product", "Description", "test-product", 29.99m, "USD", categoryId);

        _productRepository.GetBySlugAsync("test-product", Arg.Any<CancellationToken>())
            .Returns((Product?)null);
        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(Category.Create("Test Category", "Desc", "test-category"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _productRepository.Received(1).Add(Arg.Any<Product>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_When_SlugAlreadyExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            "Test Product", "Description", "existing-slug", 29.99m, "USD", categoryId);

        var existingProduct = Product.Create("Existing", "Desc", "existing-slug", 10m, "USD", categoryId);
        _productRepository.GetBySlugAsync("existing-slug", Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("SlugExists");
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_CategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            "Test Product", "Description", "test-product", 29.99m, "USD", categoryId);

        _productRepository.GetBySlugAsync("test-product", Arg.Any<CancellationToken>())
            .Returns((Product?)null);
        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("NotFound");
    }
}
