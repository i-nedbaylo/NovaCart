using MediatR;
using NovaCart.BuildingBlocks.Common;

namespace NovaCart.BuildingBlocks.CQRS;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
