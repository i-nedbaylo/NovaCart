using FluentValidation;

namespace NovaCart.Services.Ordering.Application.Commands;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    private const int MaxItems = 100;

    public CreateOrderValidator()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.");

        RuleFor(x => x.ShippingAddress.Street)
            .NotEmpty().WithMessage("Street is required.")
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.City)
            .NotEmpty().WithMessage("City is required.")
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.State)
            .NotEmpty().WithMessage("State is required.")
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.Country)
            .NotEmpty().WithMessage("Country is required.")
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.ShippingAddress.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .When(x => x.ShippingAddress is not null);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items.Count <= MaxItems)
            .WithMessage($"An order cannot contain more than {MaxItems} items.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
