using Microsoft.EntityFrameworkCore;
using NovaCart.ServiceDefaults;
using NovaCart.Services.Catalog.API;
using NovaCart.Services.Catalog.Application;
using NovaCart.Services.Catalog.Infrastructure;
using NovaCart.Services.Catalog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCatalogApplication();

var connectionString = builder.Configuration.GetConnectionString("catalogdb")
    ?? throw new InvalidOperationException("Connection string 'catalogdb' not found.");

builder.Services.AddCatalogInfrastructure(connectionString);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
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

app.MapCatalogEndpoints();

app.Run();

