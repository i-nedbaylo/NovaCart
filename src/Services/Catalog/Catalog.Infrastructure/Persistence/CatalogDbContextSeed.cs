using Microsoft.EntityFrameworkCore;
using NovaCart.Services.Catalog.Domain.Entities;

namespace NovaCart.Services.Catalog.Infrastructure.Persistence;

public static class CatalogDbContextSeed
{
    public static readonly Guid ElectronicsId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000001");
    public static readonly Guid ClothingId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000002");
    public static readonly Guid BooksId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000003");
    public static readonly Guid HomeGardenId = Guid.Parse("a1b2c3d4-0001-0001-0001-000000000004");

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedCategories(modelBuilder);
        SeedProducts(modelBuilder);
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            CreateCategory(ElectronicsId, "Electronics", "Electronic devices and gadgets", "electronics"),
            CreateCategory(ClothingId, "Clothing", "Apparel and fashion items", "clothing"),
            CreateCategory(BooksId, "Books", "Books and publications", "books"),
            CreateCategory(HomeGardenId, "Home & Garden", "Home and garden products", "home-garden"));
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(b =>
        {
            var products = new[]
            {
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000001", "Wireless Bluetooth Headphones", "Premium noise-cancelling wireless headphones with 30-hour battery life.", "wireless-bluetooth-headphones", ElectronicsId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000002", "Smartphone Stand", "Adjustable aluminum smartphone stand for desk use.", "smartphone-stand", ElectronicsId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000003", "USB-C Hub Adapter", "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", "usb-c-hub-adapter", ElectronicsId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000004", "Mechanical Keyboard", "RGB mechanical keyboard with Cherry MX switches.", "mechanical-keyboard", ElectronicsId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000005", "Cotton T-Shirt", "100% organic cotton crew-neck t-shirt.", "cotton-t-shirt", ClothingId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000006", "Denim Jeans", "Classic straight-fit denim jeans.", "denim-jeans", ClothingId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000007", "Running Sneakers", "Lightweight running sneakers with cushioned sole.", "running-sneakers", ClothingId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000008", "Clean Code", "A Handbook of Agile Software Craftsmanship by Robert C. Martin.", "clean-code", BooksId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000009", "Domain-Driven Design", "Tackling Complexity in the Heart of Software by Eric Evans.", "domain-driven-design", BooksId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000010", "The Pragmatic Programmer", "Your Journey To Mastery by David Thomas and Andrew Hunt.", "the-pragmatic-programmer", BooksId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000011", "Ceramic Plant Pot", "Handcrafted ceramic plant pot with drainage hole.", "ceramic-plant-pot", HomeGardenId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000012", "LED Desk Lamp", "Dimmable LED desk lamp with touch control.", "led-desk-lamp", HomeGardenId),
                CreateProduct("a1b2c3d4-0002-0001-0001-000000000013", "Garden Tool Set", "5-piece stainless steel garden tool set.", "garden-tool-set", HomeGardenId),
            };

            b.HasData(products);

            b.OwnsOne(p => p.Price).HasData(
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000001"), Amount = 79.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000002"), Amount = 24.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000003"), Amount = 39.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000004"), Amount = 129.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000005"), Amount = 19.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000006"), Amount = 49.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000007"), Amount = 89.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000008"), Amount = 34.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000009"), Amount = 44.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000010"), Amount = 39.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000011"), Amount = 22.50m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000012"), Amount = 34.99m, Currency = "USD" },
                new { ProductId = Guid.Parse("a1b2c3d4-0002-0001-0001-000000000013"), Amount = 45.00m, Currency = "USD" }
            );
        });
    }

    private static object CreateCategory(Guid id, string name, string description, string slug)
    {
        return new
        {
            Id = id,
            Name = name,
            Description = description,
            Slug = slug,
            ParentCategoryId = (Guid?)null,
            CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = (DateTimeOffset?)null
        };
    }

    private static object CreateProduct(string id, string name, string description, string slug, Guid categoryId)
    {
        return new
        {
            Id = Guid.Parse(id),
            Name = name,
            Description = description,
            Slug = slug,
            ImageUrl = (string?)null,
            Status = Domain.ValueObjects.ProductStatus.Active,
            CategoryId = categoryId,
            CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = (DateTimeOffset?)null
        };
    }
}
