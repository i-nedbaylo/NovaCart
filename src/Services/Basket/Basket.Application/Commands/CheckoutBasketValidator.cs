using FluentValidation;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class CheckoutBasketValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketValidator()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.");
    }
}
