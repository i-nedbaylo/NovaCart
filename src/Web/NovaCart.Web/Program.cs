using MudBlazor.Services;
using NovaCart.ServiceDefaults;
using NovaCart.Web;
using NovaCart.Web.Client.Pages;
using NovaCart.Web.Client.Services;
using NovaCart.Web.Components;
using NovaCart.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

builder.Services.AddHttpClient<CatalogService>(client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

builder.Services.AddHttpClient<OrderService>(client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

builder.Services.AddHttpClient<BasketService>(client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

// Client services registered with Gateway for Server-side InteractiveAuto rendering.
// When running on WASM, these same services use origin HttpClient via BFF proxy.
builder.Services.AddHttpClient<BasketClientService>(client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

builder.Services.AddHttpClient<OrderClientService>(client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

// Named HttpClient for BFF proxy — forwards WASM /api/* requests to Gateway
builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri("https+http://gateway");
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// BFF proxy: forwards /api/* requests from WASM components to Gateway
app.MapBffApiProxy();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NovaCart.Web.Client._Imports).Assembly);

app.Run();
