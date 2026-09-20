using FluentValidation;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class CheckoutBasketValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketValidator()
    {
        RuleFor(x => x.BasketRevision).NotEmpty();
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.").MaximumLength(200);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.").MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.").MaximumLength(100);

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.").MaximumLength(100);

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.").MaximumLength(20);
    }
}
