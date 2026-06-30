using FluentValidation;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class UpdateBasketValidator : AbstractValidator<UpdateBasketCommand>
{
    private const int MaxItems = 100;

    public UpdateBasketValidator()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required.")
            .Must(items => items.Count <= MaxItems)
            .WithMessage($"A basket cannot contain more than {MaxItems} items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
