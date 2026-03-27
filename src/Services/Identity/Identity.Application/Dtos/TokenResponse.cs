namespace NovaCart.Services.Identity.Application.Dtos;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
