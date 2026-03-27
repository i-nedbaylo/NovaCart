using MediatR;
using NovaCart.BuildingBlocks.Common;

namespace NovaCart.BuildingBlocks.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
