namespace NovaCart.Services.Identity.Contracts;

public sealed record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);
