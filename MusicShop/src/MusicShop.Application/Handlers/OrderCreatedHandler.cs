using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.Events;
using MusicShop.Domain.Entities.Orders;
using System.Linq;

namespace MusicShop.Application.Handlers;

public sealed class OrderCreatedHandler(
    IOrderRepository orderRepository,
    IEmailService emailService,
    ILogger<OrderCreatedHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Processing OrderCreatedEvent for Order {OrderId}", notification.OrderId);

        Order? order = await orderRepository.GetByIdWithDetailsAsync(notification.OrderId, ct);
        
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} not found while processing OrderCreatedEvent.", notification.OrderId);
            return;
        }

        string subject = $"Order Confirmation - #{order.Id.ToString().ToUpper()[..8]}";
        
        string itemsHtml = string.Join("", order.OrderItems.Select(item => $@"
            <tr>
                <td style='padding: 10px; border-bottom: 1px solid #eee;'>{item.ProductNameSnapshot}</td>
                <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>${item.PriceSnapshot:F2}</td>
            </tr>"));

        string body = $@"
            <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                <h2 style='color: #333; text-align: center;'>Thank You for Your Order!</h2>
                <p>Hello {order.RecipientName},</p>
                <p>We've received your order <strong>#{order.Id}</strong> and it is currently <strong>Pending</strong>.</p>
                
                <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                    <thead>
                        <tr style='background-color: #f8f9fa;'>
                            <th style='padding: 10px; text-align: left;'>Item</th>
                            <th style='padding: 10px; text-align: center;'>Qty</th>
                            <th style='padding: 10px; text-align: right;'>Price</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='2' style='padding: 10px; font-weight: bold; text-align: right;'>Total:</td>
                            <td style='padding: 10px; font-weight: bold; text-align: right; color: #4f46e5;'>${order.TotalAmount:F2}</td>
                        </tr>
                    </tfoot>
                </table>
                
                <div style='background-color: #f1f5f9; padding: 15px; border-radius: 8px; margin-top: 20px;'>
                    <h4 style='margin-top: 0;'>Shipping Address:</h4>
                    <p style='margin-bottom: 0; font-size: 14px;'>
                        {order.ShippingAddress}<br/>
                        Phone: {order.Phone}
                    </p>
                </div>

                <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;' />
                <p style='font-size: 12px; color: #666; text-align: center;'>
                    If you have any questions, please reply to this email.<br/>
                    <strong>MusicShop Team</strong>
                </p>
            </div>";

        try
        {
            await emailService.SendEmailAsync(order.Email, subject, body);
            logger.LogInformation("Order confirmation email sent successfully for Order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order confirmation email for Order {OrderId}", order.Id);
            throw; // Rethrow to trigger Hangfire retry
        }
    }
}
