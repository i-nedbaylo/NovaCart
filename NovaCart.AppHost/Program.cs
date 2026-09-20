using System.Security.Cryptography;

var builder = DistributedApplication.CreateBuilder(args);

// Secrets are injected from AppHost configuration (env vars / user-secrets), never committed to
// service appsettings. The JWT signing key is generated per run when not provided, so no working
// secret lives in the repo; the admin seed password keeps a demo default but stays overridable.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
var adminPassword = builder.Configuration["AdminUser:Password"] ?? "Admin123!";

// Infrastructure
var postgres = builder.AddPostgres("postgres").WithImageTag("17.4");
var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");
var identityDb = postgres.AddDatabase("identitydb");
var paymentDb = postgres.AddDatabase("paymentdb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithImageTag("4.1");
var redis = builder.AddRedis("redis").WithImageTag("7.4")
    .WithDataVolume()
    .WithArgs("--appendonly", "yes", "--appendfsync", "always", "--maxmemory-policy", "noeviction");

// Services
var catalogApi = builder.AddProject<Projects.NovaCart_Services_Catalog_API>("catalog-api", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithReference(catalogDb)
    .WithReference(rabbitmq)
    .WaitFor(catalogDb)
    .WaitFor(rabbitmq);

var orderingApi = builder.AddProject<Projects.NovaCart_Services_Ordering_API>("ordering-api", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithReference(orderingDb)
    .WithReference(rabbitmq)
    .WithReference(catalogApi)
    .WaitFor(orderingDb)
    .WaitFor(rabbitmq)
    .WaitFor(catalogApi);

var identityApi = builder.AddProject<Projects.NovaCart_Services_Identity_API>("identity-api", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithReference(identityDb)
    .WithReference(rabbitmq)
    .WaitFor(identityDb)
    .WaitFor(rabbitmq);

var basketApi = builder.AddProject<Projects.NovaCart_Services_Basket_API>("basket-api", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(catalogApi)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(catalogApi);

var paymentApi = builder.AddProject<Projects.NovaCart_Services_Payment_API>("payment-api", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithReference(paymentDb)
    .WithReference(rabbitmq)
    .WaitFor(paymentDb)
    .WaitFor(rabbitmq);

// API Gateway
var gateway = builder.AddProject<Projects.NovaCart_ApiGateway_Yarp>("gateway", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(identityApi)
    .WithReference(basketApi);

// Web (BFF)
var web = builder.AddProject<Projects.NovaCart_Web>("web", launchProfileName: null)
    .WithHttpEndpoint(name: "http")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithReference(gateway);

// AppHost local runs must work without ignored per-project launchSettings.json files.
if (builder.ExecutionContext.IsRunMode)
    foreach (var service in new[] { catalogApi, orderingApi, identityApi, basketApi, paymentApi, gateway, web })
        service.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Inject the shared JWT signing key into every token-validating service (Identity also signs).
foreach (var service in new[] { catalogApi, orderingApi, identityApi, basketApi })
    service.WithEnvironment("Jwt__Secret", jwtSecret);

// Identity seeds the demo admin user with this (overridable) password.
identityApi.WithEnvironment("AdminUser__Password", adminPassword);

builder.Build().Run();
