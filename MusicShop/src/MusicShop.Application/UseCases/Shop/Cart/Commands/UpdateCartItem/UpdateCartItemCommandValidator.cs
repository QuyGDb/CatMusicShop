using FluentValidation;

namespace MusicShop.Application.UseCases.Shop.Cart.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CartItemId)
            .NotEmpty().WithMessage("Cart item ID is required.");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100 per item.");
    }
}
