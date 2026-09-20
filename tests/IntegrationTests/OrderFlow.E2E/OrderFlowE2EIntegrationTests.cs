using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCart.Tests.OrderFlow.E2E;

/// <summary>
/// Full end-to-end order cycle over the real distributed app. Aspire.Hosting.Testing boots the
/// entire AppHost (PostgreSQL, RabbitMQ, Redis + every service + gateway) via Docker, then the
/// test drives the public HTTP surface and asserts the RabbitMQ event chain completes:
/// <c>BasketCheckout → OrderCreated → Payment → Order status updated</c>.
///
/// Payment is configured for deterministic success, followed by a simulated refund.
/// The test proves the chain runs end to end. HTTP steps are retried to absorb
/// the transient gateway timeouts that happen while the stack is still warming up under load.
/// Missing infrastructure or startup failures fail the test.
/// </summary>
[Trait("Category", "Integration")]
public sealed class OrderFlowE2EIntegrationTests
{
    private static readonly string[] RequiredResources =
        ["identity-api", "catalog-api", "basket-api", "ordering-api", "payment-api", "gateway", "web"];

    private static readonly TimeSpan RetryWindow = TimeSpan.FromSeconds(60);

    // A real seeded catalog product ("Wireless Bluetooth Headphones", $79.99). Pricing is now
    // resolved server-side from Catalog, so the basket/order must reference an existing product.
    private static readonly Guid SeededProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000001");
    private const decimal SeededProductPrice = 79.99m;
    private const int Quantity = 2;

    [Fact]
    public async Task BasketCheckout_Should_Create_Order_And_Reach_Terminal_Payment_Status()
    {
        var app = await StartAppAsync();
        await using var _ = app;

        var notifications = app.Services.GetRequiredService<ResourceNotificationService>();
        using (var startupCts = new CancellationTokenSource(TimeSpan.FromMinutes(3)))
        {
            // Wait for Healthy (not just Running): Running means the process launched, but the
            // service may not be accepting HTTP yet. Healthy means its /health endpoint passes,
            // so the first request below won't race a still-starting service into a 504.
            foreach (var resource in RequiredResources)
                await notifications.WaitForResourceHealthyAsync(resource, startupCts.Token);
        }

        using var client = app.CreateHttpClient("gateway", "http");
        client.Timeout = TimeSpan.FromSeconds(30);

        // Static SSR pagination must work as ordinary HTTP navigation without a live circuit.
        using var web = app.CreateHttpClient("web", "http");
        var firstPage = await web.GetStringAsync("/catalog?page=1");
        var secondPage = await web.GetStringAsync("/catalog?page=2");
        firstPage.Should().Contain("href=\"/catalog?page=2\"");
        var firstLinks = System.Text.RegularExpressions.Regex.Matches(firstPage, "href=\"/catalog/([a-f0-9-]{36})\"")
            .Select(m => m.Groups[1].Value).ToArray();
        var secondLinks = System.Text.RegularExpressions.Regex.Matches(secondPage, "href=\"/catalog/([a-f0-9-]{36})\"")
            .Select(m => m.Groups[1].Value).ToArray();
        firstLinks.Should().HaveCount(12);
        secondLinks.Should().ContainSingle();
        secondLinks.Intersect(firstLinks).Should().BeEmpty();

        var email = $"e2e-{Guid.NewGuid():N}@novacart.test";
        const string password = "Passw0rd!";

        // 1. Register a fresh buyer.
        var register = await SendWithRetryAsync(() => client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Password = password,
            FirstName = "E2E",
            LastName = "Tester"
        }));
        register.EnsureSuccessStatusCode();
        var buyerId = (await register.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 2. Sign in and authorize all subsequent calls.
        var login = await SendWithRetryAsync(() => client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        }));
        login.EnsureSuccessStatusCode();
        var token = await login.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        // 3. Fill the basket with a real catalog product. The client sends only id + quantity;
        // the server prices it from Catalog. The route buyer id must match the token subject.
        var putBasket = await SendWithRetryAsync(() => client.PutAsJsonAsync($"/api/v1/baskets/{buyerId}", new
        {
            Items = new[]
            {
                new { ProductId = SeededProductId, Quantity = Quantity }
            }
        }));
        putBasket.EnsureSuccessStatusCode();
        var basket = (await putBasket.Content.ReadFromJsonAsync<BasketResponse>())!;

        // Both requests refer to the same displayed basket revision and must create one order.
        var checkoutRequest = new
        {
            BasketRevision = basket.Revision,
            Street = "1 Test Street",
            City = "Testville",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        };
        var checkouts = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ =>
            client.PostAsJsonAsync("/api/v1/baskets/checkout", checkoutRequest)));
        foreach (var checkout in checkouts)
        {
            checkout.StatusCode.Should().Be(HttpStatusCode.Accepted);
            checkout.Dispose();
        }

        // 5. The event chain is eventually consistent — poll until the order is terminal.
        var order = await PollForTerminalOrderAsync(client, TimeSpan.FromSeconds(120));

        order.Should().NotBeNull("checkout should produce an order through the RabbitMQ event chain");
        order!.Items.Should().ContainSingle();
        order.TotalAmount.Should().Be(SeededProductPrice * Quantity);
        order.Status.Should().Be("Paid");
        var orders = await client.GetFromJsonAsync<PagedResult<OrderDto>>("/api/v1/orders?pageNumber=1&pageSize=10");
        orders!.TotalCount.Should().Be(1);

        using var cancel = await client.PutAsync($"/api/v1/orders/{order.Id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var deadline = DateTime.UtcNow.AddSeconds(60);
        do
        {
            var current = await client.GetFromJsonAsync<OrderDto>($"/api/v1/orders/{order.Id}");
            if (current!.Status == "Cancelled") return;
            await Task.Delay(500);
        } while (DateTime.UtcNow < deadline);
        throw new TimeoutException("Payment cancellation did not complete.");
    }

    /// <summary>
    /// Retries an idempotent-ish request on transient failures (connection errors and 5xx, including
    /// the 504 the gateway returns while an upstream service is still warming up under load).
    /// </summary>
    private static async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> send)
    {
        var deadline = DateTime.UtcNow.Add(RetryWindow);
        HttpResponseMessage? last = null;

        while (true)
        {
            try
            {
                last = await send();
                if (last.IsSuccessStatusCode || (int)last.StatusCode < 500)
                    return last;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Transient connection error / client timeout while the stack warms up.
            }

            if (DateTime.UtcNow >= deadline)
                return last ?? throw new TimeoutException("Request did not complete within the retry window.");

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }

    private static async Task<OrderDto?> PollForTerminalOrderAsync(HttpClient client, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync("/api/v1/orders?pageNumber=1&pageSize=10");
                if (response.IsSuccessStatusCode)
                {
                    var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
                    var order = paged?.Items.FirstOrDefault();
                    if (order is not null && order.Status is "Paid" or "Cancelled")
                        return order;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                // Transient while the chain is processing.
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        return null;
    }

    private static async Task<DistributedApplication> StartAppAsync()
    {
        DistributedApplication? app = null;
        try
        {
            // Cold start pulls the PostgreSQL/RabbitMQ/Redis images and launches every service,
            // so allow a generous window before failing the test.
            using var startupCts = new CancellationTokenSource(TimeSpan.FromMinutes(6));
            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.NovaCart_AppHost>(startupCts.Token);
            // Isolate the orchestrator's TLS certificate from the developer's Windows store.
            // DCP still uses TLS and validates its per-run certificate through kubeconfig.
            builder.Configuration["ASPIRE_DCP_USE_DEVELOPER_CERTIFICATE"] = "false";
            builder.Services.AddLogging(logging => logging.AddSimpleConsole().SetMinimumLevel(LogLevel.Warning));
            builder.CreateResourceBuilder<ProjectResource>("payment-api")
                .WithEnvironment("PaymentSimulation__SuccessRatePercent", "100");
            app = await builder.BuildAsync(startupCts.Token);
            await app.StartAsync(startupCts.Token);
            return app;
        }
        catch (Exception)
        {
            if (app is not null)
                await app.DisposeAsync();

            throw; // Configuration, build and startup failures must fail CI, never self-skip.
        }
    }

    private sealed record BasketResponse(Guid Revision);

    private sealed record IdResponse(Guid Id);

    private sealed record TokenResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

    private sealed record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);

    private sealed record OrderDto(Guid Id, string Status, decimal TotalAmount, List<OrderItemDto> Items);

    private sealed record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
}
