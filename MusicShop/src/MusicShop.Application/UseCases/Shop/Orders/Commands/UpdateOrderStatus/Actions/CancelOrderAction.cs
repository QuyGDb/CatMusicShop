using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus.Actions;

public sealed class CancelOrderAction(
    IStripeService stripeService,
    ILogger<CancelOrderAction> logger) : IOrderStatusAction
{
    public bool CanHandle(OrderStatus from, OrderStatus to) 
        => to == OrderStatus.Cancelled;

    public async Task ExecuteAsync(Order order, UpdateOrderStatusCommand request, CancellationToken ct)
    {
        // Only restore stock and refund if the order was already confirmed or further in the lifecycle
        if (order.Status is OrderStatus.Confirmed or OrderStatus.Shipped or OrderStatus.Delivered)
        {
            // 1. Restore stock
            foreach (OrderItem orderItem in order.OrderItems)
            {
                if (orderItem.Product != null && !orderItem.Product.IsPreorder)
                {
                    orderItem.Product.StockQty += orderItem.Quantity;
                }
            }

            // 2. Process refund if paid
            if (order.Payment != null && order.Payment.Status == PaymentStatus.Paid && !string.IsNullOrEmpty(order.Payment.TransactionCode))
            {
                Result refundResult = await stripeService.RefundOrderAsync(order.Payment.TransactionCode, ct);
                if (refundResult.IsFailure)
                {
                    logger.LogWarning("Failed to process refund for Order {OrderId}: {Error}", order.Id, refundResult.Error.Message);
                }
                else
                {
                    order.Payment.Status = PaymentStatus.Refunded;
                }
            }
        }
    }
}
