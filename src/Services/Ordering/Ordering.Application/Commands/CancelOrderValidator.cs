using FluentValidation;

namespace NovaCart.Services.Ordering.Application.Commands;

public sealed class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
