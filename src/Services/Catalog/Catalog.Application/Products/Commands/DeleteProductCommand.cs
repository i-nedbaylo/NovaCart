using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Catalog.Application.Products.Commands;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
