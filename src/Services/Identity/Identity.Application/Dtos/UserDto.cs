namespace NovaCart.Services.Identity.Application.Dtos;

public sealed record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
