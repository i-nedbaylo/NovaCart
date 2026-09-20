using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Contracts.IntegrationEvents;
using NovaCart.Services.Ordering.Domain.Repositories;
using NovaCart.Services.Ordering.Domain.ValueObjects;
namespace NovaCart.Services.Ordering.Application.Commands;

public sealed class CancelOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork, IOutboxEventCollector events)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.BuyerId != request.BuyerId)
            return Result.Failure(Error.NotFound("Order", request.OrderId));
        if (order.Status is OrderStatus.Cancelled or OrderStatus.CancellationPending) return Result.Success();
        try { order.RequestCancellation(); }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Order.InvalidStatusTransition", ex.Message));
        }
        events.Add(new OrderCancellationRequestedIntegrationEvent {
            OrderId = order.Id, Amount = order.TotalAmount, Currency = order.Currency, CorrelationId = order.Id
        });
        orders.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
