using Microsoft.EntityFrameworkCore;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.ServiceDefaults;
using NovaCart.Services.Ordering.API;
using NovaCart.Services.Ordering.Application;
using NovaCart.Services.Ordering.Infrastructure;
using NovaCart.Services.Ordering.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOrderingApplication();

var connectionString = builder.Configuration.GetConnectionString("orderingdb")
    ?? throw new InvalidOperationException("Connection string 'orderingdb' not found.");

builder.Services.AddOrderingInfrastructure(connectionString);

builder.AddEventBus(typeof(NovaCart.Services.Ordering.Application.DependencyInjection).Assembly);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    await dbContext.Database.MigrateAsync();
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

app.MapOrderingEndpoints();

app.Run();
