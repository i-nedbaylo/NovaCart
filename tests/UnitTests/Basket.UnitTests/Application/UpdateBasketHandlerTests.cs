using FluentAssertions;
using NSubstitute;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Tests.Basket.UnitTests.Application;

public class UpdateBasketHandlerTests
{
    private readonly IBasketRepository _basketRepository;
    private readonly UpdateBasketHandler _handler;

    public UpdateBasketHandlerTests()
    {
        _basketRepository = Substitute.For<IBasketRepository>();
        _handler = new UpdateBasketHandler(_basketRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnBasketDto_When_ValidCommand()
    {
        // Arrange
        var buyerId = "buyer-1";
        var productId = Guid.NewGuid();
        var items = new List<UpdateBasketItemRequest>
        {
            new(productId, "Laptop", 999.99m, 1)
        };

        _basketRepository.UpdateBasketAsync(Arg.Any<ShoppingCart>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ShoppingCart>());

        var command = new UpdateBasketCommand(buyerId, items);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerId.Should().Be(buyerId);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].ProductId.Should().Be(productId);
        result.Value.TotalPrice.Should().Be(999.99m);
    }

    [Fact]
    public async Task Handle_Should_CreateNewCart_When_CalledWithMultipleItems()
    {
        // Arrange
        var buyerId = "buyer-1";
        var items = new List<UpdateBasketItemRequest>
        {
            new(Guid.NewGuid(), "Laptop", 999.99m, 1),
            new(Guid.NewGuid(), "Mouse", 29.99m, 2)
        };

        _basketRepository.UpdateBasketAsync(Arg.Any<ShoppingCart>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ShoppingCart>());

        var command = new UpdateBasketCommand(buyerId, items);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalPrice.Should().Be(999.99m + 29.99m * 2);
    }
}
