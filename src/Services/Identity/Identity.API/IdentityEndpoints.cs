using System.Security.Claims;
using MediatR;
using NovaCart.BuildingBlocks.Common;
using NovaCart.Services.Identity.Application.Commands;
using NovaCart.Services.Identity.Application.Dtos;
using NovaCart.Services.Identity.Application.Queries;

namespace NovaCart.Services.Identity.API;

public static class IdentityEndpoints
{
    public static WebApplication MapIdentityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, ISender sender) =>
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/auth/me", new { id = result.Value })
                : MapError(result.Error);
        });

        group.MapPost("/login", async (LoginRequest request, ISender sender) =>
        {
            var command = new LoginCommand(request.Email, request.Password);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, ISender sender) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapGet("/me", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var result = await sender.Send(new GetCurrentUserQuery(userId));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        }).RequireAuthorization();

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

public sealed record RefreshTokenRequest(string RefreshToken);
