using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Identity.Application.Dtos;

namespace NovaCart.Services.Identity.Application.Queries;

public sealed record GetCurrentUserQuery(string UserId) : IQuery<UserDto>;
