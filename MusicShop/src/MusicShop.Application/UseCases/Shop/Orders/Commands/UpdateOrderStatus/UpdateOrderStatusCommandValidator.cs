using FluentValidation;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.");

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters.");
    }
}
