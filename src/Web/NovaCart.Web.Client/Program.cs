using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using NovaCart.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// HttpClient pointing to origin — BFF proxy on Blazor server forwards /api/* to Gateway
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<BasketClientService>();
builder.Services.AddScoped<OrderClientService>();

await builder.Build().RunAsync();
