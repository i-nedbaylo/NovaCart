using FluentValidation;

namespace NovaCart.Services.Basket.Application.Commands;

public sealed class DeleteBasketValidator : AbstractValidator<DeleteBasketCommand>
{
    public DeleteBasketValidator()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");
    }
}
