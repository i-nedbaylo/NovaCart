using System.Net.Http.Json;
using NovaCart.Web.Services.Models;

namespace NovaCart.Web.Services;

public sealed class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TokenResponse?> LoginAsync(LoginModel model, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", model, cancellationToken);

        // Invalid credentials (and similar) surface as a null result rather than an exception.
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> RegisterAsync(RegisterModel model, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsJsonAsync("/api/v1/auth/register", model, cancellationToken);
    }

    public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/refresh", new { RefreshToken = refreshToken }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
    }
}
