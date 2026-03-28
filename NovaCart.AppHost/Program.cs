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
    .WaitFor(rabbitmq);

var orderingApi = builder.AddProject<Projects.NovaCart_Services_Ordering_API>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(rabbitmq)
    .WaitFor(orderingDb)
    .WaitFor(rabbitmq);

var identityApi = builder.AddProject<Projects.NovaCart_Services_Identity_API>("identity-api")
    .WithReference(identityDb)
    .WithReference(rabbitmq)
    .WaitFor(identityDb)
    .WaitFor(rabbitmq);

var basketApi = builder.AddProject<Projects.NovaCart_Services_Basket_API>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

var paymentApi = builder.AddProject<Projects.NovaCart_Services_Payment_API>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbitmq)
    .WaitFor(paymentDb)
    .WaitFor(rabbitmq);

// API Gateway
var gateway = builder.AddProject<Projects.NovaCart_ApiGateway_Yarp>("gateway")
    .WithReference(catalogApi)
    .WithReference(orderingApi)
    .WithReference(identityApi)
    .WithReference(basketApi);

// Web (BFF)
builder.AddProject<Projects.NovaCart_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(gateway);

builder.Build().Run();
