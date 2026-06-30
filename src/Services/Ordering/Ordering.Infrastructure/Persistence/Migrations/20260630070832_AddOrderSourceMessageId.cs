using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaCart.Services.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSourceMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "source_message_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_source_message_id",
                table: "orders",
                column: "source_message_id",
                unique: true,
                filter: "source_message_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_source_message_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "source_message_id",
                table: "orders");
        }
    }
}
