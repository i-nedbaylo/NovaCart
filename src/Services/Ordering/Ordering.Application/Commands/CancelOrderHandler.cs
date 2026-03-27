using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Ordering.Domain.Repositories;

namespace NovaCart.Services.Ordering.Application.Commands;

public sealed class CancelOrderHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("Order", request.OrderId));

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation("Order.InvalidStatusTransition", ex.Message));
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
