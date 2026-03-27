using FluentValidation;

namespace NovaCart.Services.Catalog.Application.Products.Commands;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Product slug is required.")
            .MaximumLength(250).WithMessage("Product slug must not exceed 250 characters.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.PriceAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Price amount must be non-negative.");

        RuleFor(x => x.PriceCurrency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");
    }
}
