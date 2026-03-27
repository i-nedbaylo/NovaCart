using NovaCart.BuildingBlocks.CQRS;

namespace NovaCart.Services.Identity.Application.Commands;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<Guid>;
