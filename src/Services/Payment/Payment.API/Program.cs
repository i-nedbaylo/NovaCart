using Microsoft.EntityFrameworkCore;
using NovaCart.BuildingBlocks.EventBus;
using NovaCart.ServiceDefaults;
using NovaCart.Services.Payment.Application;
using NovaCart.Services.Payment.Infrastructure;
using NovaCart.Services.Payment.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddPaymentApplication();

var connectionString = builder.Configuration.GetConnectionString("paymentdb")
    ?? throw new InvalidOperationException("Connection string 'paymentdb' not found.");

builder.Services.AddPaymentInfrastructure(connectionString);

builder.AddEventBus(typeof(NovaCart.Services.Payment.Application.DependencyInjection).Assembly);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
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

app.Run();
