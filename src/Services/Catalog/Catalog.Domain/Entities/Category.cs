using NovaCart.BuildingBlocks.Common;

namespace NovaCart.Services.Catalog.Domain.Entities;

public sealed class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public Guid? ParentCategoryId { get; private set; }
    public string Slug { get; private set; } = null!;

    public Category? ParentCategory { get; private set; }
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private readonly List<Product> _products = [];

    private Category() { }

    public static Category Create(string name, string description, string slug, Guid? parentCategoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var category = new Category
        {
            Name = name,
            Description = description,
            Slug = slug.ToLowerInvariant(),
            ParentCategoryId = parentCategoryId
        };

        return category;
    }

    public void Update(string name, string description, string slug, Guid? parentCategoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name;
        Description = description;
        Slug = slug.ToLowerInvariant();
        ParentCategoryId = parentCategoryId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
