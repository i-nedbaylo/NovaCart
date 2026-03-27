using Microsoft.AspNetCore.Identity;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.CQRS;
using NovaCart.Services.Identity.Application.Dtos;
using NovaCart.Services.Identity.Application.Interfaces;
using NovaCart.Services.Identity.Domain.Entities;

namespace NovaCart.Services.Identity.Application.Commands;

public sealed class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public RefreshTokenHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Result<TokenResponse>.Failure(Error.Validation("Auth.InvalidRefreshToken", "Invalid or expired refresh token."));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _tokenService.GetAccessTokenExpiration();

        user.UpdateRefreshToken(newRefreshToken, expiresAt.AddDays(7));
        await _userManager.UpdateAsync(user);

        return Result<TokenResponse>.Success(new TokenResponse(accessToken, newRefreshToken, expiresAt));
    }
}
