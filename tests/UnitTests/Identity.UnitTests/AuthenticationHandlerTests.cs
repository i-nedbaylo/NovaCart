using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NovaCart.Services.Identity.Application.Commands;
using NovaCart.Services.Identity.Application.Interfaces;
using NovaCart.Services.Identity.Domain.Entities;
namespace NovaCart.Tests.Identity.UnitTests;

public class AuthenticationHandlerTests
{
    private readonly ApplicationUser user = ApplicationUser.Create("buyer@example.test", "Test", "Buyer");
    private readonly ITokenService tokens = Substitute.For<ITokenService>();
    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly UserManager<ApplicationUser> manager;
    public AuthenticationHandlerTests()
    {
        manager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(), new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), Substitute.For<IServiceProvider>(), NullLogger<UserManager<ApplicationUser>>.Instance);
        user.UpdateRefreshToken("old-refresh", DateTimeOffset.UtcNow.AddDays(1));
        manager.FindByEmailAsync(user.Email!).Returns(user);
        manager.CheckPasswordAsync(user, "password").Returns(true);
        manager.GetRolesAsync(user).Returns(new List<string> { "Customer" });
        manager.ResetAccessFailedCountAsync(user).Returns(IdentityResult.Success);
        manager.UpdateAsync(user).Returns(IdentityResult.Success);
        users.FindByRefreshTokenAsync("old-refresh", Arg.Any<CancellationToken>()).Returns(user);
        tokens.GenerateAccessToken(user, Arg.Any<IList<string>>()).Returns("access");
        tokens.GenerateRefreshToken().Returns("new-refresh");
        tokens.GetAccessTokenExpiration().Returns(DateTimeOffset.UtcNow.AddHours(1));
    }
    [Fact]
    public async Task Login_RejectsLockedAccountBeforeCheckingPassword()
    {
        manager.IsLockedOutAsync(user).Returns(true);
        var result = await new LoginHandler(manager, tokens).Handle(new(user.Email!, "password"), default);
        result.IsFailure.Should().BeTrue();
        await manager.DidNotReceive().CheckPasswordAsync(user, Arg.Any<string>());
    }
    [Fact]
    public async Task InvalidPassword_IsRegisteredWithIdentityLockout()
    {
        manager.CheckPasswordAsync(user, "password").Returns(false);
        var result = await new LoginHandler(manager, tokens).Handle(new(user.Email!, "password"), default);
        result.IsFailure.Should().BeTrue();
        await manager.Received(1).AccessFailedAsync(user);
    }
    [Fact]
    public async Task Login_DoesNotReturnUnpersistedTokens()
    {
        manager.UpdateAsync(user).Returns(IdentityResult.Failed(new IdentityError { Code = "ConcurrencyFailure" }));
        var result = await new LoginHandler(manager, tokens).Handle(new(user.Email!, "password"), default);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.ConcurrentUpdate");
    }
    [Fact]
    public async Task Refresh_DoesNotReturnUnpersistedTokens()
    {
        manager.UpdateAsync(user).Returns(IdentityResult.Failed(new IdentityError { Code = "ConcurrencyFailure" }));
        var result = await new RefreshTokenHandler(manager, tokens, users).Handle(new("old-refresh"), default);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.ConcurrentUpdate");
    }
    [Fact]
    public async Task Refresh_RejectsLockedAccount()
    {
        manager.IsLockedOutAsync(user).Returns(true);
        var result = await new RefreshTokenHandler(manager, tokens, users).Handle(new("old-refresh"), default);
        result.IsFailure.Should().BeTrue();
        await manager.DidNotReceive().UpdateAsync(user);
    }
}
