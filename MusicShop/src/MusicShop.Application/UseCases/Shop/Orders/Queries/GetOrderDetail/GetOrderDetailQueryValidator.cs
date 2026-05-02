using FluentValidation;

namespace MusicShop.Application.UseCases.Shop.Orders.Queries.GetOrderDetail;

public sealed class GetOrderDetailQueryValidator : AbstractValidator<GetOrderDetailQuery>
{
    public GetOrderDetailQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
