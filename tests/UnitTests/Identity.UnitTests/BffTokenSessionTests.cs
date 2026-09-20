using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using NovaCart.Web;
using NovaCart.Web.Services.Models;
namespace NovaCart.Tests.Identity.UnitTests;

public class BffTokenSessionTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class Handler(Clock clock) : HttpMessageHandler, IHttpClientFactory
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Interlocked.Increment(ref Calls);
            await Task.Delay(10, ct);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new TokenResponse("renewed-access", "renewed-refresh", clock.Now.AddHours(1))) };
        }
        public HttpClient CreateClient(string name) => new(this, false) { BaseAddress = new Uri("https://gateway.test") };
    }
    private sealed class Auth(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }
    private static ClaimsPrincipal Principal(Clock clock) => new(new ClaimsIdentity([
        new Claim(BffAuth.SessionIdClaim, Guid.NewGuid().ToString()),
        new Claim(BffAuth.AccessTokenClaim, "original-access"),
        new Claim(BffAuth.RefreshTokenClaim, "original-refresh"),
        new Claim(BffAuth.ExpiresAtClaim, clock.Now.AddHours(1).ToUnixTimeSeconds().ToString())
    ], "cookie"));

    [Fact]
    public async Task ExistingCircuit_RenewsWithoutAnotherHttpCookieRequest()
    {
        var clock = new Clock();
        var principal = Principal(clock);
        using var handler = new Handler(clock);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new BffTokenSessionStore(cache, handler, clock);
        var accessor = new ServerAccessTokenAccessor(new Auth(principal), store);
        (await accessor.GetAccessTokenAsync()).Should().Be("original-access");
        clock.Now = clock.Now.AddMinutes(66);
        (await accessor.GetAccessTokenAsync()).Should().Be("renewed-access");
        handler.Calls.Should().Be(1);
        principal.FindFirst(BffAuth.AccessTokenClaim)!.Value.Should().Be("original-access");
    }
    [Fact]
    public async Task ParallelHttpAndCircuitRequests_ShareOneRotation()
    {
        var clock = new Clock();
        var principal = Principal(clock);
        clock.Now = clock.Now.AddMinutes(58);
        using var handler = new Handler(clock);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new BffTokenSessionStore(cache, handler, clock);
        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => store.GetAsync(principal)));
        results.Should().OnlyContain(t => t!.AccessToken == "renewed-access");
        handler.Calls.Should().Be(1);
    }
    [Fact]
    public async Task Logout_RevokesExistingCircuit()
    {
        var clock = new Clock();
        var principal = Principal(clock);
        using var handler = new Handler(clock);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new BffTokenSessionStore(cache, handler, clock);
        await store.GetAsync(principal);
        await store.RevokeAsync(principal);
        (await store.GetAsync(principal)).Should().BeNull();
    }
}
