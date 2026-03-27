using MediatR;
using NovaCart.BuildingBlocks.Common;

namespace NovaCart.BuildingBlocks.CQRS;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
