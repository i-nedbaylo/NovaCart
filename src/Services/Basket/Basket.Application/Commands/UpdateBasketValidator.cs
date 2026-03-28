using FluentValidation;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class UpdateBasketValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketValidator()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.");

            item.RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
