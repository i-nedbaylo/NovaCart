using Microsoft.AspNetCore.Identity;

namespace NovaCart.Services.Identity.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private ApplicationUser() { }

    public static ApplicationUser Create(string email, string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateRefreshToken(string refreshToken, DateTimeOffset expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
