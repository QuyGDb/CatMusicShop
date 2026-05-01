using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Events;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;
using MusicShop.Domain.Enums;

namespace MusicShop.Application.Handlers;

public sealed class StripePaymentSucceededHandler(
    IMediator mediator,
    ILogger<StripePaymentSucceededHandler> logger) : INotificationHandler<StripePaymentSucceededEvent>
{
    public async Task Handle(StripePaymentSucceededEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Stripe payment succeeded for Order {OrderId}. Updating status.", notification.OrderId);

        UpdateOrderStatusCommand command = new(
            notification.OrderId,
            OrderStatus.Confirmed,
            null,
            notification.StripeSessionId);

        await mediator.Send(command, ct);
    }
}
