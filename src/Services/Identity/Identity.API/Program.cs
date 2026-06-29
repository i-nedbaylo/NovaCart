using Microsoft.EntityFrameworkCore;
using NovaCart.ServiceDefaults;
using NovaCart.Services.Identity.API;
using NovaCart.Services.Identity.Application;
using NovaCart.Services.Identity.Infrastructure;
using NovaCart.Services.Identity.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddIdentityApplication();

var connectionString = builder.Configuration.GetConnectionString("identitydb")
    ?? throw new InvalidOperationException("Connection string 'identitydb' not found.");

builder.Services.AddIdentityInfrastructure(connectionString, builder.Configuration);

builder.AddJwtAuthentication();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
    await dbContext.Database.MigrateAsync();

    await IdentityDbContextSeed.SeedAdminUserAsync(app.Services);
}

app.UseAuthentication();
app.UseAuthorization();

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

app.MapIdentityEndpoints();

app.Run();
