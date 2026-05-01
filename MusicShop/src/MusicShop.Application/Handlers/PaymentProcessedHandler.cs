using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Events;

namespace MusicShop.Application.Handlers;

public sealed class PaymentProcessedHandler(
    ILogger<PaymentProcessedHandler> logger) : INotificationHandler<PaymentProcessedEvent>
{
    public async Task Handle(PaymentProcessedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Processing PaymentProcessedEvent for Order {OrderId}", notification.OrderId);
        
        // Example: Update Order Status to Paid
        
        await Task.CompletedTask;
    }
}
