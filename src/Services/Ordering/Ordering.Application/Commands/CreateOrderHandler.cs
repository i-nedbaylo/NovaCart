using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Application.Abstractions;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Entities;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;

namespace NovaCart.Services.Ordering.Application.Commands;

public sealed class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxEventCollector _outboxEventCollector;
    private readonly ICatalogProductReader _catalog;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOutboxEventCollector outboxEventCollector,
        ICatalogProductReader catalog)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _outboxEventCollector = outboxEventCollector;
        _catalog = catalog;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _catalog.GetActiveProductsAsync(productIds, cancellationToken);

        var address = Address.Create(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.ZipCode);

        var order = Order.Create(request.BuyerId, address);

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                return Result<Guid>.Failure(Error.Validation(
                    "Order.UnknownProduct",
                    $"Product '{item.ProductId}' was not found or is not available."));
            }

            // Name and price are authoritative from Catalog, never from the client request.
            order.AddItem(item.ProductId, product.Name, product.Price, item.Quantity);
        }

        _orderRepository.Add(order);

        _outboxEventCollector.Add(new OrderCreatedIntegrationEvent
        {
            OrderId = order.Id,
            BuyerId = order.BuyerId,
            TotalAmount = order.TotalAmount,
            Currency = "USD",
            CorrelationId = order.Id
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
