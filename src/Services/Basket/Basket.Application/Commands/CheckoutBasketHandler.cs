using MassTransit;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Contracts.IntegrationEvents;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class CheckoutBasketHandler : ICommandHandler<CheckoutBasketCommand>
{
    private readonly IBasketRepository _basketRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckoutBasketHandler(
        IBasketRepository basketRepository,
        IPublishEndpoint publishEndpoint)
    {
        _basketRepository = basketRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetBasketAsync(request.BuyerId, cancellationToken);

        if (basket is null || basket.Items.Count == 0)
        {
            return Result.Failure(Error.NotFound("Basket.NotFound", $"Basket for buyer '{request.BuyerId}' is empty or not found."));
        }

        var integrationEvent = new BasketCheckoutIntegrationEvent
        {
            BuyerId = request.BuyerId,
            TotalPrice = basket.TotalPrice,
            Street = request.Street,
            City = request.City,
            State = request.State,
            Country = request.Country,
            ZipCode = request.ZipCode,
            Items = basket.Items.Select(i => new BasketCheckoutItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        // Clear the basket after checkout
        await _basketRepository.DeleteBasketAsync(request.BuyerId, cancellationToken);

        return Result.Success();
    }
}
