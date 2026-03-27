using MediatR;
using NovaCart.BuildingBlocks.Common;
using NovaCart.Services.Ordering.Application.Commands;
using NovaCart.Services.Ordering.Application.Dtos;
using NovaCart.Services.Ordering.Application.Queries;

namespace NovaCart.Services.Ordering.API;

public static class OrderingEndpoints
{
    public static WebApplication MapOrderingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders");

        group.MapGet("/", async (
            ISender sender,
            int? pageNumber,
            int? pageSize,
            Guid? buyerId) =>
        {
            var query = new GetOrdersQuery(
                pageNumber ?? 1,
                pageSize ?? 10,
                buyerId);

            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapPost("/", async (CreateOrderRequest request, ISender sender) =>
        {
            var command = new CreateOrderCommand(
                request.BuyerId,
                request.ShippingAddress,
                request.Items);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/orders/{result.Value}", new { id = result.Value })
                : MapError(result.Error);
        });

        group.MapPut("/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CancelOrderCommand(id));

            return result.IsSuccess
                ? Results.NoContent()
                : MapError(result.Error);
        });

        return app;
    }

    private static IResult MapError(Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Results.Conflict(new { error.Code, error.Message }),
        ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
        _ => Results.BadRequest(new { error.Code, error.Message })
    };
}
