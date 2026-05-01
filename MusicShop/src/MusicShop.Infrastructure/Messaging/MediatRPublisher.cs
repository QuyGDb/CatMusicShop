using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.Events;
using MusicShop.Application.Common.Constants;

namespace MusicShop.Infrastructure.Messaging;

public sealed class MediatRPublisher : IMessagePublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<MediatRPublisher> _logger;

    private static readonly IReadOnlyDictionary<string, Type> TypeMap =
        new Dictionary<string, Type>
        {
            [MessageTypes.Orders.Created]     = typeof(OrderCreatedEvent),
            [MessageTypes.Payments.Processed] = typeof(PaymentProcessedEvent),
            [MessageTypes.Stripe.PaymentSucceeded] = typeof(StripePaymentSucceededEvent),
        };

    public MediatRPublisher(IMediator mediator, ILogger<MediatRPublisher> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    public async Task PublishAsync(string messageType, string jsonPayload, CancellationToken ct = default)
    {
        if (!TypeMap.TryGetValue(messageType, out Type? type))
        {
            _logger.LogWarning("Unknown message type: {MessageType}. Skipping.", messageType);
            return;
        }

        object? notification = JsonSerializer.Deserialize(jsonPayload, type);
        if (notification is not INotification mediatrNotification)
        {
            throw new InvalidOperationException($"Failed to deserialize: {messageType}");
        }

        _logger.LogInformation("Publishing {Type} via MediatR", messageType);

        await _mediator.Publish(mediatrNotification, ct);
    }
}
