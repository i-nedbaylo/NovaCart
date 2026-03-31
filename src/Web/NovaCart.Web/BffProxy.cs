namespace NovaCart.Web;

/// <summary>
/// BFF proxy that forwards API requests from WASM components to the API Gateway.
/// When Blazor runs in WebAssembly mode, HttpClient calls go to the Blazor host origin.
/// This proxy maps /api/{path} requests and forwards them to the Gateway service.
/// </summary>
public static class BffProxy
{
    public static WebApplication MapBffApiProxy(this WebApplication app)
    {
        app.Map("/api/{**path}", async (HttpContext context, IHttpClientFactory clientFactory) =>
        {
            var client = clientFactory.CreateClient("Gateway");
            var path = context.GetRouteValue("path")?.ToString();
            var targetUrl = $"/api/{path}{context.Request.QueryString}";

            using var request = new HttpRequestMessage(
                new HttpMethod(context.Request.Method), targetUrl);

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

            if (context.Request.Headers.TryGetValue("Authorization", out var auth))
            {
                request.Headers.TryAddWithoutValidation("Authorization", auth.ToString());
            }

            using var response = await client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            context.Response.StatusCode = (int)response.StatusCode;

            if (response.Content.Headers.ContentType is not null)
            {
                context.Response.ContentType = response.Content.Headers.ContentType.ToString();
            }

            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        });

        return app;
    }
}
