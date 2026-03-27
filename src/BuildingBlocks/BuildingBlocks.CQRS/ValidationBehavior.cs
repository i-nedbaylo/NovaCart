using FluentValidation;
using MediatR;
using NovaCart.BuildingBlocks.Common;

namespace NovaCart.BuildingBlocks.CQRS;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        if (errors.Count != 0)
        {
            var firstError = errors[0];

            if (typeof(TResponse) == typeof(Result))
                return (TResponse)(object)Result.Failure(firstError);

            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.Failure), [typeof(Error)])!;

            return (TResponse)failureMethod.Invoke(null, [firstError])!;
        }

        return await next(cancellationToken);
    }
}
