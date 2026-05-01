using MusicShop.Domain.Common;

namespace MusicShop.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error CartEmpty = new(
        "Order.CartEmpty",
        "Your cart is empty. Please add items before placing an order.",
        ErrorType.Validation);

    public static readonly Error NotFound = new(
        "Order.NotFound",
        "The specified order was not found.",
        ErrorType.NotFound);

    public static readonly Error CannotCancel = new(
        "Order.CannotCancel",
        "This order cannot be cancelled in its current state.",
        ErrorType.Conflict);

    public static readonly Error InvalidStatusTransition = new(
        "Order.InvalidStatusTransition",
        "This status transition is not allowed.",
        ErrorType.Conflict);
}
