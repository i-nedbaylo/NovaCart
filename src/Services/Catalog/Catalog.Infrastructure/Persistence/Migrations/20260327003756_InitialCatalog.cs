using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NovaCart.Services.Catalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "created_at", "description", "name", "parent_category_id", "slug", "updated_at" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Electronic devices and gadgets", "Electronics", null, "electronics", null },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000002"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Apparel and fashion items", "Clothing", null, "clothing", null },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000003"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Books and publications", "Books", null, "books", null },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000004"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Home and garden products", "Home & Garden", null, "home-garden", null }
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "id", "category_id", "created_at", "description", "image_url", "name", "slug", "status", "updated_at", "price_amount", "price_currency" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Premium noise-cancelling wireless headphones with 30-hour battery life.", null, "Wireless Bluetooth Headphones", "wireless-bluetooth-headphones", "Active", null, 79.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Adjustable aluminum smartphone stand for desk use.", null, "Smartphone Stand", "smartphone-stand", "Active", null, 24.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader.", null, "USB-C Hub Adapter", "usb-c-hub-adapter", "Active", null, 39.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "RGB mechanical keyboard with Cherry MX switches.", null, "Mechanical Keyboard", "mechanical-keyboard", "Active", null, 129.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000005"), new Guid("a1b2c3d4-0001-0001-0001-000000000002"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "100% organic cotton crew-neck t-shirt.", null, "Cotton T-Shirt", "cotton-t-shirt", "Active", null, 19.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000006"), new Guid("a1b2c3d4-0001-0001-0001-000000000002"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Classic straight-fit denim jeans.", null, "Denim Jeans", "denim-jeans", "Active", null, 49.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000007"), new Guid("a1b2c3d4-0001-0001-0001-000000000002"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Lightweight running sneakers with cushioned sole.", null, "Running Sneakers", "running-sneakers", "Active", null, 89.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000008"), new Guid("a1b2c3d4-0001-0001-0001-000000000003"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A Handbook of Agile Software Craftsmanship by Robert C. Martin.", null, "Clean Code", "clean-code", "Active", null, 34.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000009"), new Guid("a1b2c3d4-0001-0001-0001-000000000003"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tackling Complexity in the Heart of Software by Eric Evans.", null, "Domain-Driven Design", "domain-driven-design", "Active", null, 44.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000010"), new Guid("a1b2c3d4-0001-0001-0001-000000000003"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Your Journey To Mastery by David Thomas and Andrew Hunt.", null, "The Pragmatic Programmer", "the-pragmatic-programmer", "Active", null, 39.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000011"), new Guid("a1b2c3d4-0001-0001-0001-000000000004"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Handcrafted ceramic plant pot with drainage hole.", null, "Ceramic Plant Pot", "ceramic-plant-pot", "Active", null, 22.50m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000012"), new Guid("a1b2c3d4-0001-0001-0001-000000000004"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Dimmable LED desk lamp with touch control.", null, "LED Desk Lamp", "led-desk-lamp", "Active", null, 34.99m, "USD" },
                    { new Guid("a1b2c3d4-0002-0001-0001-000000000013"), new Guid("a1b2c3d4-0001-0001-0001-000000000004"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "5-piece stainless steel garden tool set.", null, "Garden Tool Set", "garden-tool-set", "Active", null, 45.00m, "USD" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_slug",
                table: "products",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
