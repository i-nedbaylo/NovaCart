using Microsoft.AspNetCore.Identity;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Identity.Application.Dtos;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Application.Queries;

public sealed class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetCurrentUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return Result<UserDto>.Failure(Error.NotFound("User", request.UserId));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles.ToList());

        return Result<UserDto>.Success(dto);
    }
}
