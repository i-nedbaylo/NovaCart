using System.Net.Http.Headers;

namespace NovaCart.Web;

/// <summary>
/// BFF proxy that forwards API requests from WASM components to the API Gateway.
/// When Blazor runs in WebAssembly mode, HttpClient calls go to the Blazor host origin.
/// This proxy maps /api/{path} requests and forwards them to the Gateway service.
/// </summary>
public static class BffProxy
{
    private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host", "Connection", "Proxy-Connection", "Keep-Alive",
        "Transfer-Encoding", "TE", "Trailer", "Upgrade",
        "Proxy-Authorization", "Proxy-Authenticate",
        "Content-Length", "Content-Type"
    };

    public static WebApplication MapBffApiProxy(this WebApplication app)
    {
        app.Map("/api/{**path}", async (HttpContext context, IHttpClientFactory clientFactory) =>
        {
            var client = clientFactory.CreateClient("Gateway");
            var routePath = context.GetRouteValue("path")?.ToString();
            var targetUrl = $"/api/{routePath}{context.Request.QueryString}";

            using var request = new HttpRequestMessage(
                new HttpMethod(context.Request.Method), targetUrl);

            // Copy request body
            if (context.Request.ContentLength > 0
                || context.Request.Headers.ContainsKey("Transfer-Encoding"))
            {
                request.Content = new StreamContent(context.Request.Body);

                if (context.Request.ContentType is not null)
                {
                    request.Content.Headers.TryAddWithoutValidation(
                        "Content-Type", context.Request.ContentType);
                }
            }

            // Forward request headers (excluding hop-by-hop)
            foreach (var header in context.Request.Headers)
            {
                if (HopByHopHeaders.Contains(header.Key))
                    continue;

                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())
                    && request.Content is not null)
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            using var response = await client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            context.Response.StatusCode = (int)response.StatusCode;

            // Forward response headers
            CopyResponseHeaders(response.Headers, context.Response.Headers);
            CopyResponseHeaders(response.Content.Headers, context.Response.Headers);

            // Content-Type is excluded from generic header copying (handled separately for requests)
            // but must be set explicitly on the response for correct content negotiation
            if (response.Content.Headers.ContentType is not null)
            {
                context.Response.ContentType = response.Content.Headers.ContentType.ToString();
            }

            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        });

        return app;
    }

    private static void CopyResponseHeaders(HttpHeaders source, IHeaderDictionary destination)
    {
        foreach (var header in source)
        {
            if (HopByHopHeaders.Contains(header.Key))
                continue;

            destination.Append(header.Key, header.Value.ToArray());
        }
    }
}
