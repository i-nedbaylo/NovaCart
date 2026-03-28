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

        if (basket is null)
        {
            return Result.Failure(Error.NotFound("Basket", request.BuyerId));
        }

        if (basket.Items.Count == 0)
        {
            return Result.Failure(Error.Validation("Basket.Empty", $"Basket for buyer '{request.BuyerId}' is empty."));
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

        // NOTE: Simplified for demo purposes. In production, Publish + Delete should be atomic
        // via Outbox Pattern (Phase 2.6) to prevent duplicate events on retry/failure.
        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        await _basketRepository.DeleteBasketAsync(request.BuyerId, cancellationToken);

        return Result.Success();
    }
}
