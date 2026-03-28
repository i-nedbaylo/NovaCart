using FluentValidation.TestHelper;
using NovaCart.Services.Catalog.Application.Products.Commands;

namespace NovaCart.Tests.Catalog.UnitTests.Application;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public async Task Validate_Should_HaveError_When_NameIsEmpty()
    {
        // Arrange
        var command = new CreateProductCommand(
            "", "Description", "test-product", 29.99m, "USD", Guid.NewGuid());

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_Should_HaveError_When_PriceIsNegative()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product", "Description", "test-product", -5m, "USD", Guid.NewGuid());

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PriceAmount);
    }
}
