using Microsoft.AspNetCore.Identity;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Identity.Application.Dtos;
using NovaCart.Services.Identity.Application.Interfaces;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Application.Commands;

public sealed class LoginHandler : ICommandHandler<LoginCommand, TokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public LoginHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<Result<TokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<TokenResponse>.Failure(Error.Validation("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result<TokenResponse>.Failure(Error.Validation("Auth.InvalidCredentials", "Invalid email or password."));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _tokenService.GetAccessTokenExpiration();

        user.UpdateRefreshToken(refreshToken, expiresAt.AddDays(7));
        await _userManager.UpdateAsync(user);

        return Result<TokenResponse>.Success(new TokenResponse(accessToken, refreshToken, expiresAt));
    }
}
