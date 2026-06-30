namespace NovaCart.Web.Client.Services;

/// <summary>
/// Resolves the currently authenticated buyer. Has a server-side and a WASM-side implementation
/// so the same interactive component works in both render locations (Blazor Auto). The access
/// token is never exposed here — only the buyer's public identity.
/// </summary>
public interface IUserService
{
    Task<BuyerInfo?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

public sealed record BuyerInfo(string Id, string Email, string Name);
