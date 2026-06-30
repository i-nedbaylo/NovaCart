using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Catalog.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace NovaCart.Tests.Catalog.IntegrationTests;

/// <summary>
/// Spins up a real PostgreSQL instance in a throwaway Docker container (via Testcontainers),
/// applies the Catalog migrations, and shares it across the test collection. If Docker is not
/// available the fixture degrades gracefully — <see cref="IsAvailable"/> becomes false and the
/// tests skip instead of failing, so the suite stays green on machines without Docker.
/// </summary>
public sealed class CatalogPostgresFixture : IAsyncLifetime
{
    public const string DockerUnavailableSkipReason =
        "Integration test requires Docker with a running engine. Install/start Docker Desktop " +
        "(https://docs.docker.com/desktop/); the test then starts PostgreSQL in a container automatically.";

    // Allows overriding the image for air-gapped/mirrored registries (e.g. in CI).
    private const string ImageEnvironmentVariable = "TESTCONTAINERS_POSTGRES_IMAGE";
    private const string DefaultImage = "postgres:16-alpine";

    private PostgreSqlContainer? _container;

    public string? ConnectionString { get; private set; }

    public bool IsAvailable => ConnectionString is not null;

    public string? UnavailableReason { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder()
                .WithImage(GetContainerImage())
                // A dedicated application database (not the default maintenance DB) so the schema
                // can be recreated independently.
                .WithDatabase("catalog_integration")
                .WithCleanUp(true)
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }
        catch (Exception ex)
        {
            // Docker not installed/running, image pull blocked, daemon timeout, etc.
            // Record the reason and let dependent tests skip rather than fail the run.
            UnavailableReason = $"{DockerUnavailableSkipReason} (reason: {ex.Message})";
            ConnectionString = null;
            _container = null;
            return;
        }

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }

    public CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(ConnectionString!)
            .Options;

        return new CatalogDbContext(options);
    }

    private static string GetContainerImage() =>
        Environment.GetEnvironmentVariable(ImageEnvironmentVariable) ?? DefaultImage;
}

[CollectionDefinition(CollectionName)]
public sealed class CatalogPostgresCollection : ICollectionFixture<CatalogPostgresFixture>
{
    public const string CollectionName = "CatalogPostgres";
}
