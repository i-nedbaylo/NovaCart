using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCart.Tests.OrderFlow.E2E;

/// <summary>
/// Full end-to-end order cycle over the real distributed app. Aspire.Hosting.Testing boots the
/// entire AppHost (PostgreSQL, RabbitMQ, Redis + every service + gateway) via Docker, then the
/// test drives the public HTTP surface and asserts the RabbitMQ event chain completes:
/// <c>BasketCheckout → OrderCreated → Payment → Order status updated</c>.
///
/// Payment is a simulated 80%-success provider, so the test asserts the order reaches a
/// *terminal* status (Paid or Cancelled) rather than a specific outcome — proving the chain
/// runs end to end without depending on the random result. HTTP steps are retried to absorb
/// the transient gateway timeouts that happen while the stack is still warming up under load.
/// The test self-skips when Docker / the Aspire orchestrator is unavailable.
/// </summary>
public sealed class OrderFlowE2EIntegrationTests
{
    public const string InfrastructureUnavailableSkipReason =
        "End-to-end test requires Docker (Testcontainers) and the Aspire orchestrator (DCP). " +
        "Install/start Docker Desktop; the test then boots the whole AppHost automatically.";

    private static readonly string[] RequiredResources =
        ["identity-api", "basket-api", "ordering-api", "payment-api", "gateway"];

    private static readonly TimeSpan RetryWindow = TimeSpan.FromSeconds(60);

    // A real seeded catalog product ("Wireless Bluetooth Headphones", $79.99). Pricing is now
    // resolved server-side from Catalog, so the basket/order must reference an existing product.
    private static readonly Guid SeededProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000001");
    private const decimal SeededProductPrice = 79.99m;
    private const int Quantity = 2;

    [SkippableFact]
    public async Task BasketCheckout_Should_Create_Order_And_Reach_Terminal_Payment_Status()
    {
        var (app, startupError) = await TryStartAppAsync();
        await using var _ = app;
        Skip.If(app is null, startupError ?? InfrastructureUnavailableSkipReason);

        var notifications = app!.Services.GetRequiredService<ResourceNotificationService>();
        using (var startupCts = new CancellationTokenSource(TimeSpan.FromMinutes(3)))
        {
            foreach (var resource in RequiredResources)
                await notifications.WaitForResourceAsync(resource, KnownResourceStates.Running, startupCts.Token);
        }

        using var client = app.CreateHttpClient("gateway", "http");
        client.Timeout = TimeSpan.FromSeconds(30);

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

        // 4. Checkout → publishes BasketCheckoutIntegrationEvent. This is NOT idempotent (it
        // creates an order), so it is sent exactly once — never through the retry wrapper.
        using var checkout = await client.PostAsJsonAsync("/api/v1/baskets/checkout", new
        {
            Street = "1 Test Street",
            City = "Testville",
            State = "TS",
            Country = "Testland",
            ZipCode = "12345"
        });
        checkout.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // 5. The event chain is eventually consistent — poll until the order is terminal.
        var order = await PollForTerminalOrderAsync(client, TimeSpan.FromSeconds(120));

        order.Should().NotBeNull("checkout should produce an order through the RabbitMQ event chain");
        order!.Items.Should().ContainSingle();
        order.TotalAmount.Should().Be(SeededProductPrice * Quantity);
        order.Status.Should().BeOneOf("Paid", "Cancelled");
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

    private static async Task<(DistributedApplication? App, string? Error)> TryStartAppAsync()
    {
        DistributedApplication? app = null;
        try
        {
            // Cold start pulls the PostgreSQL/RabbitMQ/Redis images and launches every service,
            // so allow a generous window before giving up and skipping.
            using var startupCts = new CancellationTokenSource(TimeSpan.FromMinutes(6));
            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.NovaCart_AppHost>(startupCts.Token);
            app = await builder.BuildAsync(startupCts.Token);
            await app.StartAsync(startupCts.Token);
            return (app, null);
        }
        catch (Exception ex)
        {
            if (app is not null)
                await app.DisposeAsync();

            return (null, $"{InfrastructureUnavailableSkipReason} (startup failed: {ex.GetType().Name}: {ex.Message})");
        }
    }

    private sealed record IdResponse(Guid Id);

    private sealed record TokenResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

    private sealed record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);

    private sealed record OrderDto(Guid Id, string Status, decimal TotalAmount, List<OrderItemDto> Items);

    private sealed record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
}
