using MediatR;
using NovaCart.BuildingBlocks.Common;

namespace NovaCart.BuildingBlocks.CQRS;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
