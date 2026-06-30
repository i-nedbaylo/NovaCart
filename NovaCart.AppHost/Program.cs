var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");
var identityDb = postgres.AddDatabase("identitydb");
var paymentDb = postgres.AddDatabase("paymentdb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");
var redis = builder.AddRedis("redis");

// Services
var catalogApi = builder.AddProject<Projects.NovaCart_Services_Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(rabbitmq)
    .WaitFor(catalogDb)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

var orderingApi = builder.AddProject<Projects.NovaCart_Services_Ordering_API>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(rabbitmq)
    .WithReference(catalogApi)
    .WaitFor(orderingDb)
    .WaitFor(rabbitmq)
    .WaitFor(catalogApi)
    .WithHttpHealthCheck("/health");

var identityApi = builder.AddProject<Projects.NovaCart_Services_Identity_API>("identity-api")
    .WithReference(identityDb)
    .WithReference(rabbitmq)
    .WaitFor(identityDb)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

var basketApi = builder.AddProject<Projects.NovaCart_Services_Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(catalogApi)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(catalogApi)
    .WithHttpHealthCheck("/health");

var paymentApi = builder.AddProject<Projects.NovaCart_Services_Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbitmq)
    .WaitFor(paymentDb)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

// API Gateway
var gateway = builder.AddProject<Projects.NovaCart_ApiGateway_Yarp>("gateway")
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(identityApi)
    .WithReference(basketApi)
    // The gateway's own /health is only a self-check (YARP doesn't aggregate upstream health),
    // so gate its startup on the upstreams being healthy — otherwise it could route before they
    // are ready and return proxy 5xx.
    .WaitFor(catalogApi)
    .WaitFor(orderingApi)
    .WaitFor(identityApi)
    .WaitFor(basketApi)
    .WithHttpHealthCheck("/health");

// Web (BFF)
builder.AddProject<Projects.NovaCart_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(gateway)
    .WaitFor(gateway);

builder.Build().Run();
