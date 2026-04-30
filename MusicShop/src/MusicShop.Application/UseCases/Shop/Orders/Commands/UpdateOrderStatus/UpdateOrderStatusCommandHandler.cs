using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IEmailService emailService,
    IStripeService stripeService,
    IUnitOfWork unitOfWork,
    ILogger<UpdateOrderStatusCommandHandler> logger) : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating Order {OrderId} status to {Status}", request.OrderId, request.Status);
        Order? order = await orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        OrderStatus targetStatus = request.Status;

        // Pre-check for StockIssue on Confirmation
        if (targetStatus == OrderStatus.Confirmed && order.Status == OrderStatus.Pending)
        {
            bool hasStockIssue = order.OrderItems.Any(oi => oi.Product != null && !oi.Product.IsPreorder && oi.Product.StockQty < oi.Quantity);
            if (hasStockIssue)
            {
                targetStatus = OrderStatus.StockIssue;
            }
        }

        if (order.Status == targetStatus)
        {
            return Result.Success();
        }

        // Logic 1: Transition TO Confirmed (Payment Success)
        if (targetStatus == OrderStatus.Confirmed && order.Status == OrderStatus.Pending)
        {
            // 1. Deduct stock now that we have real money
            foreach (OrderItem orderItem in order.OrderItems)
            {
                if (orderItem.Product != null && !orderItem.Product.IsPreorder)
                {
                    orderItem.Product.StockQty -= orderItem.Quantity;
                }
            }

            // 2. Mark payment as Paid
            if (order.Payment != null)
            {
                order.Payment.Status = PaymentStatus.Paid;
                order.Payment.PaidAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.TransactionCode))
                {
                    order.Payment.TransactionCode = request.TransactionCode;
                }
            }

            // 3. Clear user's cart (Snapshot: we use order.UserId to find the cart)
            MusicShop.Domain.Entities.Orders.Cart? cart = await cartRepository.GetByUserIdAsync(order.UserId, cancellationToken);
            if (cart != null)
            {
                await cartRepository.ClearCartAsync(cart.Id, cancellationToken);
            }
        }
        else if (targetStatus == OrderStatus.StockIssue && order.Status == OrderStatus.Pending)
        {
            logger.LogWarning("Order {OrderId} has a StockIssue. Initiating refund for transaction {TransactionCode}", order.Id, request.TransactionCode);

            if (order.Payment != null)
            {
                if (!string.IsNullOrEmpty(request.TransactionCode))
                {
                    Result refundResult = await stripeService.RefundOrderAsync(request.TransactionCode, cancellationToken);
                    if (refundResult.IsFailure)
                    {
                        logger.LogWarning("Failed to process automatic refund for StockIssue Order {OrderId}: {Error}", order.Id, refundResult.Error.Message);
                    }
                    else
                    {
                        order.Payment.Status = PaymentStatus.Refunded;
                    }
                    order.Payment.TransactionCode = request.TransactionCode;
                }
                order.Payment.PaidAt = DateTime.UtcNow;
            }

            MusicShop.Domain.Entities.Orders.Cart? cart = await cartRepository.GetByUserIdAsync(order.UserId, cancellationToken);
            if (cart != null)
            {
                await cartRepository.ClearCartAsync(cart.Id, cancellationToken);
            }
        }
        else if (targetStatus == OrderStatus.Cancelled &&
                 (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered))
        {
            foreach (OrderItem orderItem in order.OrderItems)
            {
                if (orderItem.Product != null && !orderItem.Product.IsPreorder)
                {
                    orderItem.Product.StockQty += orderItem.Quantity;
                }
            }

            if (order.Payment != null && order.Payment.Status == PaymentStatus.Paid && !string.IsNullOrEmpty(order.Payment.TransactionCode))
            {
                Result refundResult = await stripeService.RefundOrderAsync(order.Payment.TransactionCode, cancellationToken);
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

        order.Status = targetStatus;

        if (!string.IsNullOrEmpty(request.TrackingNumber))
        {
            order.TrackingNumber = request.TrackingNumber;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Send Email Notification
        try
        {
            await SendStatusUpdateEmailAsync(order, cancellationToken);
        }
        catch (Exception ex)
        {
            // Don't fail the whole request if email fails, but log it
            logger.LogError(ex, "Failed to send order status update email for Order {OrderId}", order.Id);
        }

        return Result.Success();
    }

    private async Task SendStatusUpdateEmailAsync(Order order, CancellationToken cancellationToken)
    {
        string subject = $"Order #{order.Id.ToString().ToUpper()[..8]} Status Update";
        string statusText = order.Status.ToString();
        string body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #333;'>Hello {order.RecipientName},</h2>
                <p>Your order <strong>#{order.Id}</strong> has been updated to: <span style='color: #4f46e5; font-weight: bold;'>{statusText}</span></p>
                
                {(order.Status == OrderStatus.Shipped && !string.IsNullOrEmpty(order.TrackingNumber)
                    ? $"<p>Your tracking number is: <strong>{order.TrackingNumber}</strong></p>"
                    : "")}
                
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='font-size: 12px; color: #666;'>Thank you for shopping with MusicShop!</p>
            </div>";

        await emailService.SendEmailAsync(order.Email, subject, body);
    }
}
