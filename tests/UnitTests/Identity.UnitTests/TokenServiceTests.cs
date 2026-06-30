using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaCart.Services.Identity.Domain.Entities;
using NovaCart.Services.Identity.Infrastructure.Services;

namespace NovaCart.Tests.Identity.UnitTests;

public class TokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Secret = "NovaCart-Super-Secret-Key-For-Development-Only-Min-32-Chars!",
        Issuer = "NovaCart.Identity",
        Audience = "NovaCart.Client",
        ExpirationInMinutes = 60
    };

    private const string UserId = "11111111-1111-1111-1111-111111111111";

    private static TokenService CreateSut() => new(Options.Create(Settings));

    private static ApplicationUser CreateUser()
    {
        var user = ApplicationUser.Create("user@novacart.com", "Ada", "Lovelace");
        user.Id = UserId;
        return user;
    }

    [Fact]
    public void GenerateAccessToken_Should_Be_Valid_And_Carry_User_Claims_And_Roles()
    {
        var token = CreateSut().GenerateAccessToken(CreateUser(), ["Customer", "Admin"]);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Secret))
        };

        var principal = new JwtSecurityTokenHandler()
            .ValidateToken(token, validationParameters, out _);

        principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(UserId);
        principal.FindFirstValue(ClaimTypes.Email).Should().Be("user@novacart.com");
        principal.FindFirstValue(ClaimTypes.Name).Should().Be("Ada Lovelace");
        principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .Should().BeEquivalentTo("Customer", "Admin");
    }

    [Fact]
    public void GenerateAccessToken_Should_Be_Signed_So_A_Different_Key_Fails_Validation()
    {
        var token = CreateSut().GenerateAccessToken(CreateUser(), []);

        var wrongKeyParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("A-Totally-Different-Secret-Key-With-At-Least-32-Chars!"))
        };

        var validate = () => new JwtSecurityTokenHandler()
            .ValidateToken(token, wrongKeyParameters, out _);

        validate.Should().Throw<SecurityTokenException>();
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_Unique_64_Byte_Values()
    {
        var sut = CreateSut();

        var first = sut.GenerateRefreshToken();
        var second = sut.GenerateRefreshToken();

        first.Should().NotBeNullOrWhiteSpace();
        first.Should().NotBe(second);
        Convert.FromBase64String(first).Should().HaveCount(64);
    }

    [Fact]
    public void GetAccessTokenExpiration_Should_Be_Close_To_Configured_Lifetime()
    {
        var expected = DateTimeOffset.UtcNow.AddMinutes(Settings.ExpirationInMinutes);

        var expiration = CreateSut().GetAccessTokenExpiration();

        expiration.Should().BeCloseTo(expected, TimeSpan.FromSeconds(30));
    }
}
