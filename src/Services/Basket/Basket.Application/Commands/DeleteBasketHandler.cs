using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class DeleteBasketHandler : ICommandHandler<DeleteBasketCommand>
{
    private readonly IBasketRepository _basketRepository;

    public DeleteBasketHandler(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    public async Task<Result> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        await _basketRepository.DeleteBasketAsync(request.BuyerId, cancellationToken);

        return Result.Success();
    }
}
