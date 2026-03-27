using MediatR;
using NovaCart.BuildingBlocks.Common;
using NovaCart.BuildingBlocks.Persistence;
using NovaCart.Services.Catalog.Application.Categories.Commands;
using NovaCart.Services.Catalog.Application.Categories.Dtos;
using NovaCart.Services.Catalog.Application.Categories.Queries;
using NovaCart.Services.Catalog.Application.Products.Commands;
using NovaCart.Services.Catalog.Application.Products.Dtos;
using NovaCart.Services.Catalog.Application.Products.Queries;

namespace NovaCart.Services.Catalog.API;

public static class CatalogEndpoints
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        app.MapProductEndpoints();
        app.MapCategoryEndpoints();
        return app;
    }

    private static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapGet("/", async (
            ISender sender,
            int? pageNumber,
            int? pageSize,
            Guid? categoryId,
            string? searchTerm,
            string? sortBy,
            bool? sortDescending) =>
        {
            var query = new GetProductsQuery(
                pageNumber ?? 1,
                pageSize ?? 10,
                categoryId,
                searchTerm,
                sortBy,
                sortDescending ?? false);

            var result = await sender.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapPost("/", async (CreateProductRequest request, ISender sender) =>
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Description,
                request.Slug,
                request.PriceAmount,
                request.PriceCurrency,
                request.CategoryId,
                request.ImageUrl);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/products/{result.Value}", new { id = result.Value })
                : MapError(result.Error);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender) =>
        {
            var command = new UpdateProductCommand(
                id,
                request.Name,
                request.Description,
                request.Slug,
                request.PriceAmount,
                request.PriceCurrency,
                request.CategoryId,
                request.ImageUrl);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : MapError(result.Error);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));

            return result.IsSuccess
                ? Results.NoContent()
                : MapError(result.Error);
        });
    }

    private static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCategoriesQuery());

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetCategoryByIdQuery(id));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : MapError(result.Error);
        });

        group.MapPost("/", async (CreateCategoryCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/categories/{result.Value}", new { id = result.Value })
                : MapError(result.Error);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ISender sender) =>
        {
            var command = new UpdateCategoryCommand(
                id,
                request.Name,
                request.Description,
                request.Slug,
                request.ParentCategoryId);

            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : MapError(result.Error);
        });
    }

    private static IResult MapError(Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
        ErrorType.Conflict => Results.Conflict(new { error.Code, error.Message }),
        ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
        _ => Results.BadRequest(new { error.Code, error.Message })
    };
}

public sealed record UpdateCategoryRequest(
    string Name,
    string Description,
    string Slug,
    Guid? ParentCategoryId = null);
