using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Identity.Application.Dtos;

namespace NovaCart.Services.Identity.Application.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<TokenResponse>;
