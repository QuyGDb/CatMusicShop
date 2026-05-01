using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.Events;
using MusicShop.Application.Common.Constants;

namespace MusicShop.Infrastructure.Messaging;

public sealed class MediatRPublisher(IMediator mediator, ILogger<MediatRPublisher> logger) : IMessagePublisher
{
    private static readonly Dictionary<string, Type> TypeMap =
        new()
        {
            [MessageTypes.Orders.Created] = typeof(OrderCreatedEvent),
            [MessageTypes.Stripe.PaymentSucceeded] = typeof(StripePaymentSucceededEvent),
        };

    public async Task PublishAsync(string messageType, string jsonPayload, CancellationToken ct = default)
    {
        if (!TypeMap.TryGetValue(messageType, out Type? type))
        {
            logger.LogWarning("Unknown message type: {MessageType}. Skipping.", messageType);
            return;
        }

        object? notification = JsonSerializer.Deserialize(jsonPayload, type);
        if (notification is not INotification mediatrNotification)
        {
            throw new InvalidOperationException($"Failed to deserialize: {messageType}");
        }

        logger.LogInformation("Publishing {Type} via MediatR", messageType);

        await mediator.Publish(mediatrNotification, ct);
    }
}
