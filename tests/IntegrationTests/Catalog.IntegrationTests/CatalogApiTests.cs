using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace NovaCart.Tests.Catalog.IntegrationTests;

/// <summary>
/// Boots Catalog.API in-memory (WebApplicationFactory — no Docker) to cover two cross-cutting
/// behaviors unit tests can't: (1) the dependency-aware <c>/health</c> readiness check actually
/// reflects database reachability, and (2) the admin-only authorization boundary on write
/// endpoints. The database points at an unreachable host on purpose, so <c>/health</c> is
/// Unhealthy and write handlers fail *after* authorization — all these assertions need.
/// </summary>
[Trait("Category", "Integration")]
public sealed class CatalogApiTests : IClassFixture<CatalogApiTests.Factory>
{
    private const string Secret = "NovaCart-Super-Secret-Key-For-Development-Only-Min-32-Chars!";
    private const string Issuer = "NovaCart.Identity";
    private const string Audience = "NovaCart.Client";

    private readonly Factory _factory;

    public CatalogApiTests(Factory factory) => _factory = factory;

    [Fact]
    public async Task Health_Should_Be_Unhealthy_When_Database_Unreachable()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        // The DbContext readiness check is wired into /health, so an unreachable DB → 503.
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Alive_Should_Be_Healthy_Even_When_Database_Unreachable()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/alive");

        // Liveness is self-only (not coupled to dependencies), so it stays healthy.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProduct_Should_Return_401_When_Anonymous()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/products", SampleProduct());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_Should_Return_403_When_Authenticated_But_Not_Admin()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = Bearer(CreateToken("Customer"));

        var response = await client.PostAsJsonAsync("/api/v1/products", SampleProduct());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProduct_Should_Pass_Authorization_When_Admin()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = Bearer(CreateToken("Admin"));

        var response = await client.PostAsJsonAsync("/api/v1/products", SampleProduct());

        // Admin clears the policy; the request reaches the handler (which then fails on the
        // unreachable DB). The point of this test is only that authorization let it through.
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProducts_Should_Stay_Public()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/products");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    private static object SampleProduct() => new
    {
        Name = "Test",
        Description = "Test product",
        Slug = "test-product",
        PriceAmount = 9.99m,
        PriceCurrency = "USD",
        CategoryId = Guid.NewGuid(),
        ImageUrl = "https://example.test/x.png"
    };

    private static AuthenticationHeaderValue Bearer(string token) => new("Bearer", token);

    private static string CreateToken(string role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // "Testing" (not Development) skips the startup DB migration, so the app boots even
            // though the database is unreachable. The unreachable DB is the whole point: it lets
            // us assert /health turns Unhealthy and that auth runs before the DB is touched.
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:catalogdb",
                "Host=127.0.0.1;Port=1;Database=catalog_test;Username=test;Password=test;Timeout=2;Command Timeout=2");
            builder.UseSetting("Jwt:Secret", Secret);
            builder.UseSetting("Jwt:Issuer", Issuer);
            builder.UseSetting("Jwt:Audience", Audience);
        }
    }
}
