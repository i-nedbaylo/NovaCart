using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Basket.Application.Abstractions;
using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Application.Mapping;
using NovaCart.Services.Basket.Domain.Entities;
using NovaCart.Services.Basket.Domain.Repositories;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class UpdateBasketHandler : ICommandHandler<UpdateBasketCommand, BasketDto>
{
    private readonly IBasketRepository _basketRepository;
    private readonly ICatalogProductReader _catalog;

    public UpdateBasketHandler(IBasketRepository basketRepository, ICatalogProductReader catalog)
    {
        _basketRepository = basketRepository;
        _catalog = catalog;
    }

    public async Task<Result<BasketDto>> Handle(UpdateBasketCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _catalog.GetActiveProductsAsync(productIds, cancellationToken);

        var basket = ShoppingCart.Create(request.BuyerId);

        foreach (var item in request.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                return Result<BasketDto>.Failure(Error.Validation(
                    "Basket.UnknownProduct",
                    $"Product '{item.ProductId}' was not found or is not available."));
            }

            // Name and price are authoritative from Catalog, never from the client request.
            basket.AddItem(item.ProductId, product.Name, product.Price, item.Quantity);
        }

        var updated = await _basketRepository.UpdateBasketAsync(basket, cancellationToken);

        return Result<BasketDto>.Success(BasketMapper.ToDto(updated));
    }
}
