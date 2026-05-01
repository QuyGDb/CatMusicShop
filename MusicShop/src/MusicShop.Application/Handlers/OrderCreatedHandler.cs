using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.Events;

namespace MusicShop.Application.Handlers;

public sealed class OrderCreatedHandler(
    IEmailService emailService,
    ILogger<OrderCreatedHandler> logger) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Processing OrderCreatedEvent for Order {OrderId}", notification.OrderId);
        
        // Example: Send confirmation email
        // await emailService.SendOrderConfirmationAsync(notification.UserId, ct);
        
        await Task.CompletedTask;
    }
}
