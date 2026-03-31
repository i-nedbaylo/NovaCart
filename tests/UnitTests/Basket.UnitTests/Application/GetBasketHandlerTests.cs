using FluentAssertions;
using NSubstitute;
using NovaCart.Services.Basket.Application.Queries;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Tests.Basket.UnitTests.Application;

public class GetBasketHandlerTests
{
    private readonly IBasketRepository _basketRepository;
    private readonly GetBasketHandler _handler;

    public GetBasketHandlerTests()
    {
        _basketRepository = Substitute.For<IBasketRepository>();
        _handler = new GetBasketHandler(_basketRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnBasketDto_When_BasketExists()
    {
        // Arrange
        var buyerId = "buyer-1";
        var cart = ShoppingCart.Create(buyerId);
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1);

        _basketRepository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var query = new GetBasketQuery(buyerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerId.Should().Be(buyerId);
        result.Value.Items.Should().ContainSingle();
        result.Value.TotalPrice.Should().Be(999.99m);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyBasket_When_BasketNotExists()
    {
        // Arrange
        var buyerId = "buyer-1";

        _basketRepository.GetBasketAsync(buyerId, Arg.Any<CancellationToken>())
            .Returns((ShoppingCart?)null);

        var query = new GetBasketQuery(buyerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerId.Should().Be(buyerId);
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalPrice.Should().Be(0);
    }
}
