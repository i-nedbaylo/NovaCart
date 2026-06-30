using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Catalog.Domain.Entities;
using NovaCart.Services.Catalog.Infrastructure.Persistence;
using NovaCart.Services.Catalog.Infrastructure.Repositories;

namespace NovaCart.Tests.Catalog.IntegrationTests;

/// <summary>
/// Exercises the EF Core mapping and repository against a real PostgreSQL engine — snake_case
/// tables, the owned <c>Price</c> value object, seed data, and the write path — which in-memory
/// unit tests cannot validate.
/// </summary>
[Collection(CatalogPostgresCollection.CollectionName)]
public sealed class CatalogRepositoryIntegrationTests
{
    private readonly CatalogPostgresFixture _fixture;

    public CatalogRepositoryIntegrationTests(CatalogPostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task GetPagedAsync_Should_Return_Seeded_Products_With_Category()
    {
        Skip.IfNot(_fixture.IsAvailable, _fixture.UnavailableReason ?? CatalogPostgresFixture.DockerUnavailableSkipReason);

        await using var dbContext = _fixture.CreateDbContext();
        var repository = new ProductRepository(dbContext);

        var (items, totalCount) = await repository.GetPagedAsync(pageNumber: 1, pageSize: 50);

        totalCount.Should().BeGreaterThanOrEqualTo(13);
        items.Should().Contain(p => p.Slug == "clean-code");
        items.Should().OnlyContain(p => p.Category != null);
    }

    [SkippableFact]
    public async Task GetBySlugAsync_Should_Map_Owned_Price_Value_Object()
    {
        Skip.IfNot(_fixture.IsAvailable, _fixture.UnavailableReason ?? CatalogPostgresFixture.DockerUnavailableSkipReason);

        await using var dbContext = _fixture.CreateDbContext();
        var repository = new ProductRepository(dbContext);

        var product = await repository.GetBySlugAsync("clean-code");

        product.Should().NotBeNull();
        product!.Price.Amount.Should().Be(34.99m);
        product.Price.Currency.Should().Be("USD");
    }

    [SkippableFact]
    public async Task Add_Then_GetById_Should_RoundTrip_Product_With_Owned_Price()
    {
        Skip.IfNot(_fixture.IsAvailable, _fixture.UnavailableReason ?? CatalogPostgresFixture.DockerUnavailableSkipReason);

        var product = Product.Create(
            name: "Integration Test Widget",
            description: "Created by an integration test.",
            slug: $"integration-test-widget-{Guid.NewGuid():N}",
            priceAmount: 12.34m,
            currency: "USD",
            categoryId: CatalogDbContextSeed.ElectronicsId);

        await using (var writeContext = _fixture.CreateDbContext())
        {
            writeContext.Products.Add(product);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateDbContext();
        var loaded = await readContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

        loaded.Should().NotBeNull();
        loaded!.Price.Amount.Should().Be(12.34m);
        loaded.Price.Currency.Should().Be("USD");
        loaded.Slug.Should().Be(product.Slug);
    }
}
