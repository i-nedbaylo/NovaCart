using System.Security.Claims;
using MediatR;
using NovaCart.BuildingBlocks.Common;
using NovaCart.ServiceDefaults;
using NovaCart.Services.Basket.Application.Commands;
using NovaCart.Services.Basket.Application.Dtos;
using NovaCart.Services.Basket.Application.Queries;

namespace NovaCart.Services.Basket.API;

public static class BasketEndpoints
{
    public static WebApplication MapBasketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/baskets")
            .WithTags("Baskets")
            .RequireAuthorization();

        group.MapGet("/{buyerId}", async (string buyerId, ISender sender, ClaimsPrincipal user) =>
        {
            if (EnsureOwner(user, buyerId) is { } denied)
                return denied;

            var result = await sender.Send(new GetBasketQuery(buyerId));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapPut("/{buyerId}", async (string buyerId, UpdateBasketRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            if (EnsureOwner(user, buyerId) is { } denied)
                return denied;

            var command = new UpdateBasketCommand(buyerId, request.Items);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapDelete("/{buyerId}", async (string buyerId, ISender sender, ClaimsPrincipal user) =>
        {
            if (EnsureOwner(user, buyerId) is { } denied)
                return denied;

            var result = await sender.Send(new DeleteBasketCommand(buyerId));

            return result.IsSuccess
                ? Results.NoContent()
                : MapError(result.Error);
        });

        group.MapPost("/checkout", async (CheckoutBasketRequest request, ISender sender, ClaimsPrincipal user) =>
        {
            // The buyer is taken from the token, never from the request body, so a user
            // can only ever check out their own basket.
            var buyerId = user.GetUserId();
            if (buyerId is null)
                return Results.Unauthorized();

            var command = new CheckoutBasketCommand(
                buyerId.Value.ToString(),
                request.Street,
                request.City,
                request.State,
                request.Country,
                request.ZipCode);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Accepted()
                : MapError(result.Error);
        });

        return app;
    }

    // Baskets are keyed by buyer id in the route; ensure that id matches the authenticated user.
    private static IResult? EnsureOwner(ClaimsPrincipal user, string buyerId)
    {
        var userId = user.GetUserId();
        if (userId is null)
            return Results.Unauthorized();

        return string.Equals(userId.Value.ToString(), buyerId, StringComparison.OrdinalIgnoreCase)
            ? null
            : Results.Forbid();
    }

    private static IResult MapError(Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Results.Conflict(new { error.Code, error.Message }),
        ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
        _ => Results.BadRequest(new { error.Code, error.Message })
    };
}

public sealed record CheckoutBasketRequest(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);
