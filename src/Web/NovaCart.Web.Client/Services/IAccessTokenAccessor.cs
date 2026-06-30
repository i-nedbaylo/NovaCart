namespace NovaCart.Web.Client.Services;

/// <summary>
/// Supplies the current user's access token for outbound API calls. On the server it reads the
/// token the BFF holds in the auth cookie; on WASM it returns null because the BFF proxy attaches
/// the token server-side (it is never exposed to the browser). Two implementations let the same
/// client service work in both Blazor Auto render locations.
/// </summary>
public interface IAccessTokenAccessor
{
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
