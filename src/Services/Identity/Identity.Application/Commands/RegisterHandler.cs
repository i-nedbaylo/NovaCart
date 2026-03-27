using Microsoft.AspNetCore.Identity;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Identity.Domain;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Application.Commands;

public sealed class RegisterHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result<Guid>.Failure(Error.Conflict("User.AlreadyExists", $"User with email '{request.Email}' already exists."));
        }

        var user = ApplicationUser.Create(request.Email, request.FirstName, request.LastName);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<Guid>.Failure(Error.Validation("User.CreationFailed", errors));
        }

        await _userManager.AddToRoleAsync(user, UserRoles.Customer);

        return Result<Guid>.Success(Guid.Parse(user.Id));
    }
}
