using FluentValidation;

namespace NovaCart.Services.Basket.Application.Queries;

public sealed class GetBasketQueryValidator : AbstractValidator<GetBasketQuery>
{
    public GetBasketQueryValidator()
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required.");
    }
}
