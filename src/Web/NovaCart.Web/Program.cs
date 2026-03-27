using MudBlazor.Services;
using NovaCart.ServiceDefaults;
using NovaCart.Web.Client.Pages;
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NovaCart.Web.Client._Imports).Assembly);

app.Run();
