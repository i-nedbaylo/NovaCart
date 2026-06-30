namespace NovaCart.Web.Client.Services;

/// <summary>
/// WASM implementation: the access token never reaches the browser, so there is nothing to attach
/// here — the BFF proxy adds it server-side from the auth cookie.
/// </summary>
public sealed class ClientAccessTokenAccessor : IAccessTokenAccessor
{
    public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult<string?>(null);
}
