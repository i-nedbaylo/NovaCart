using FluentAssertions;
using NSubstitute;
using NovaCart.Services.Basket.Application.Abstractions;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Tests.Basket.UnitTests.Application;

public class UpdateBasketHandlerTests
{
    [Fact]
    public async Task Handle_PreservesCurrency_AndIssuesANewRevision()
    {
        var product = new CatalogProduct(Guid.NewGuid(), "Product", 12, "EUR");
        SetupCatalog(product);
        var request = new UpdateBasketCommand("buyer", [new UpdateBasketItemRequest(product.Id, 1)]);
        var first = await _handler.Handle(request, default);
        var second = await _handler.Handle(request, default);
        first.Value.Currency.Should().Be("EUR");
        first.Value.Revision.Should().NotBeEmpty().And.NotBe(second.Value.Revision);
    }

    [Fact]
    public async Task Handle_RejectsMixedCurrenciesBeforeSaving()
    {
        var eur = new CatalogProduct(Guid.NewGuid(), "Euro", 12, "EUR");
        var usd = new CatalogProduct(Guid.NewGuid(), "Dollar", 12, "USD");
        SetupCatalog(eur, usd);
        var result = await _handler.Handle(new UpdateBasketCommand("buyer",
            [new UpdateBasketItemRequest(eur.Id, 1), new UpdateBasketItemRequest(usd.Id, 1)]), default);
        result.Error.Code.Should().Be("Basket.MixedCurrency");
        await _basketRepository.DidNotReceive().UpdateBasketAsync(Arg.Any<ShoppingCart>(), Arg.Any<CancellationToken>());
    }

    private readonly IBasketRepository _basketRepository = Substitute.For<IBasketRepository>();
    private readonly ICatalogProductReader _catalog = Substitute.For<ICatalogProductReader>();
    private readonly UpdateBasketHandler _handler;

    public UpdateBasketHandlerTests()
    {
        _basketRepository.UpdateBasketAsync(Arg.Any<ShoppingCart>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<ShoppingCart>());

        _handler = new UpdateBasketHandler(_basketRepository, _catalog);
    }

    private void SetupCatalog(params CatalogProduct[] products)
    {
        IReadOnlyDictionary<Guid, CatalogProduct> map = products.ToDictionary(p => p.Id);
        _catalog.GetActiveProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(map);
    }

    [Fact]
    public async Task Handle_Should_PriceItemFromCatalog_When_ValidCommand()
    {
        var laptop = new CatalogProduct(Guid.NewGuid(), "Laptop", 999.99m, "USD");
        SetupCatalog(laptop);

        var command = new UpdateBasketCommand("buyer-1", [new UpdateBasketItemRequest(laptop.Id, 1)]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.BuyerId.Should().Be("buyer-1");
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].ProductId.Should().Be(laptop.Id);
        result.Value.Items[0].ProductName.Should().Be("Laptop");
        result.Value.TotalPrice.Should().Be(999.99m);
    }

    [Fact]
    public async Task Handle_Should_ComputeTotalFromCatalogPrices_NotClientInput()
    {
        var laptop = new CatalogProduct(Guid.NewGuid(), "Laptop", 999.99m, "USD");
        var mouse = new CatalogProduct(Guid.NewGuid(), "Mouse", 29.99m, "USD");
        SetupCatalog(laptop, mouse);

        var command = new UpdateBasketCommand(
            "buyer-1",
            [new UpdateBasketItemRequest(laptop.Id, 1), new UpdateBasketItemRequest(mouse.Id, 2)]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalPrice.Should().Be(999.99m + 29.99m * 2);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_ProductNotInCatalog()
    {
        SetupCatalog(); // empty — product unknown or not active

        var command = new UpdateBasketCommand("buyer-1", [new UpdateBasketItemRequest(Guid.NewGuid(), 1)]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Basket.UnknownProduct");
        await _basketRepository.DidNotReceive().UpdateBasketAsync(Arg.Any<ShoppingCart>(), Arg.Any<CancellationToken>());
    }
}
