var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var postgres = builder.AddPostgres("postgres");
var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");
var identityDb = postgres.AddDatabase("identitydb");

// Services
var catalogApi = builder.AddProject<Projects.NovaCart_Services_Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

var orderingApi = builder.AddProject<Projects.NovaCart_Services_Ordering_API>("ordering-api")
    .WithReference(orderingDb)
    .WaitFor(orderingDb);

builder.Build().Run();
