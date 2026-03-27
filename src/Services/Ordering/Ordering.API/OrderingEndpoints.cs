using MediatR;
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
                : Results.BadRequest(new { error = result.Error });
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error });
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
                : Results.BadRequest(new { error = result.Error });
        });

        group.MapPut("/{id:guid}/cancel", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new CancelOrderCommand(id));

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}
