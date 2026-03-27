using FluentValidation;

namespace NovaCart.Services.Catalog.Application.Categories.Commands;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Category slug is required.")
            .MaximumLength(150).WithMessage("Category slug must not exceed 150 characters.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens.");
    }
}
