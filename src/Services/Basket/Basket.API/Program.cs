using NovaCart.BuildingBlocks.EventBus;
using NovaCart.ServiceDefaults;
using NovaCart.Services.Basket.API;
using NovaCart.Services.Basket.Application;
using NovaCart.Services.Basket.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddBasketApplication();
builder.Services.AddBasketInfrastructure();

builder.Services.AddStackExchangeRedisCache(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("redis")
        ?? throw new InvalidOperationException("Connection string 'redis' not found.");

    options.Configuration = connectionString;
    options.InstanceName = "basket:";
});

builder.AddEventBus(typeof(Program).Assembly);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            title = "An error occurred while processing your request.",
            status = 500
        });
    });
});

app.MapBasketEndpoints();

app.Run();
