using NovaCart.BuildingBlocks.Common;
using NovaCart.Services.Catalog.Domain.Events;
using NovaCart.Services.Catalog.Domain.ValueObjects;

namespace NovaCart.Services.Catalog.Domain.Entities;

public sealed class Product : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public string Slug { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public Price Price { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    private Product() { }

    public static Product Create(
        string name,
        string description,
        string slug,
        decimal priceAmount,
        string currency,
        Guid categoryId,
        string? imageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var product = new Product
        {
            Name = name,
            Description = description,
            Slug = slug.ToLowerInvariant(),
            Price = Price.Create(priceAmount, currency),
            Status = ProductStatus.Active,
            CategoryId = categoryId,
            ImageUrl = imageUrl
        };

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id));

        return product;
    }

    public void Update(
        string name,
        string description,
        string slug,
        decimal priceAmount,
        string currency,
        Guid categoryId,
        string? imageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name;
        Description = description;
        Slug = slug.ToLowerInvariant();
        Price = Price.Create(priceAmount, currency);
        CategoryId = categoryId;
        ImageUrl = imageUrl;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id));
    }

    public void Activate()
    {
        Status = ProductStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
