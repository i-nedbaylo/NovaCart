using FluentAssertions;
using NSubstitute;
using NovaCart.Services.Basket.Application.Abstractions;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;
namespace NovaCart.Tests.Basket.UnitTests.Application;

public class CheckoutBasketHandlerTests
{
    private readonly IBasketRepository baskets = Substitute.For<IBasketRepository>();
    private readonly ICatalogProductReader catalog = Substitute.For<ICatalogProductReader>();
    private readonly ICheckoutStore store = Substitute.For<ICheckoutStore>();
    private readonly ShoppingCart cart = ShoppingCart.Create("buyer-1", "EUR");
    private CheckoutBasketCommand Command => new(cart.BuyerId, "Street", "City", "State", "Country", "12345", cart.Revision);
    public CheckoutBasketHandlerTests()
    {
        cart.AddItem(Guid.NewGuid(), "Product", 10m, 2);
        baskets.GetBasketAsync(cart.BuyerId, Arg.Any<CancellationToken>()).Returns(cart);
        SetCatalog(10m);
        store.TryAcceptAsync(cart, Arg.Any<BasketCheckoutIntegrationEvent>(), Arg.Any<CancellationToken>()).Returns(true);
    }
    private void SetCatalog(decimal price)
    {
        IReadOnlyDictionary<Guid, CatalogProduct> map = new Dictionary<Guid, CatalogProduct> {
            [cart.Items[0].ProductId] = new(cart.Items[0].ProductId, "Product", price, "EUR")
        };
        catalog.GetActiveProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(map);
    }
    private CheckoutBasketHandler Handler => new(baskets, catalog, store);

    [Fact]
    public async Task Checkout_PersistsTrustedQuoteWithStableId()
    {
        var result = await Handler.Handle(Command, default);
        result.IsSuccess.Should().BeTrue();
        await store.Received(1).TryAcceptAsync(cart, Arg.Is<BasketCheckoutIntegrationEvent>(e =>
            e.Id == cart.Revision && e.Currency == "EUR" && e.PricingVersion == 1 &&
            e.Items[0].UnitPrice == 10m && e.Items[0].Quantity == 2), Arg.Any<CancellationToken>());
        await baskets.DidNotReceive().DeleteBasketAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ChangedPrice_RequiresConsentAndPreservesBasket()
    {
        SetCatalog(100m);
        var result = await Handler.Handle(Command, default);
        result.Error.Code.Should().Be("Basket.Changed");
        await store.DidNotReceive().TryAcceptAsync(Arg.Any<ShoppingCart>(), Arg.Any<BasketCheckoutIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task RemovedProduct_IsRejectedBeforeAccepting()
    {
        catalog.GetActiveProductsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, CatalogProduct>());
        var result = await Handler.Handle(Command, default);
        result.Error.Code.Should().Be("Basket.UnavailableProduct");
        await store.DidNotReceive().TryAcceptAsync(Arg.Any<ShoppingCart>(), Arg.Any<BasketCheckoutIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task RetryAfterAcceptance_DoesNotNeedBasketOrCatalog()
    {
        store.IsAcceptedAsync(cart.BuyerId, cart.Revision, Arg.Any<CancellationToken>()).Returns(true);
        (await Handler.Handle(Command, default)).IsSuccess.Should().BeTrue();
        await baskets.DidNotReceive().GetBasketAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ChangedRevision_CannotConsumeNewBasket()
    {
        (await Handler.Handle(Command with { BasketRevision = Guid.NewGuid() }, default)).Error.Code.Should().Be("Basket.Changed");
        await store.DidNotReceive().TryAcceptAsync(Arg.Any<ShoppingCart>(), Arg.Any<BasketCheckoutIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task ConcurrentUpdateDuringPricing_ReturnsConflict()
    {
        store.TryAcceptAsync(cart, Arg.Any<BasketCheckoutIntegrationEvent>(), Arg.Any<CancellationToken>()).Returns(false);
        (await Handler.Handle(Command, default)).Error.Code.Should().Be("Basket.Changed");
    }
    [Fact]
    public async Task MissingRevision_IsRejected()
    {
        (await Handler.Handle(Command with { BasketRevision = Guid.Empty }, default)).Error.Code.Should().Be("Basket.RevisionRequired");
    }
}
