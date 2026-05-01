using System.Text.Json;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.Events;
using MusicShop.Application.Common.Constants;
using MusicShop.Domain.Entities.Messaging;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Messaging;

[AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
[Queue("messages")]
public sealed class MessageProcessor : IMessageProcessor
{
    private readonly AppDbContext _db;
    private readonly IMessagePublisher _publisher;
    private readonly IMediator _mediator;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(
        AppDbContext db,
        IMessagePublisher publisher,
        IMediator mediator,
        ILogger<MessageProcessor> logger)
    {
        _db        = db;
        _publisher = publisher;
        _mediator  = mediator;
        _logger    = logger;
    }

    public async Task ProcessAsync(Guid messageId, CancellationToken ct = default)
    {
        // ── load message ───────────────────────────────
        Message? message = await _db.Messages
            .FindAsync(new object[] { messageId }, ct);

        if (message is null)
        {
            _logger.LogWarning("Message {Id} not found.", messageId);
            return;
        }

        // ── idempotency guard ──────────────────────────
        if (message.ProcessedAt.HasValue)
        {
            _logger.LogInformation("Message {Id} already processed. Skipping.", messageId);
            return;
        }

        try
        {
            switch (message.Direction)
            {
                case MessageDirection.Outbox:
                    await ProcessOutboxAsync(message, ct);
                    break;

                case MessageDirection.Inbox:
                    await ProcessInboxAsync(message, ct);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown direction: {message.Direction}");
            }

            message.ProcessedAt = DateTime.UtcNow;
            message.Error       = null;

            _logger.LogInformation(
                "Message {Id} ({Direction}/{Type}) processed successfully.",
                messageId, message.Direction, message.Type);
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message;

            _logger.LogError(ex,
                "Message {Id} ({Direction}/{Type}) failed. RetryCount={Count}",
                messageId, message.Direction, message.Type, message.RetryCount);

            throw; // Hangfire handles retry
        }
        finally
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    // ── outbox: acquire lock → publish ────────────────
    private async Task ProcessOutboxAsync(Message message, CancellationToken ct)
    {
        string lockId = Guid.NewGuid().ToString();
        bool locked = await TryAcquireLockAsync(message.Id, lockId);

        if (!locked)
        {
            _logger.LogInformation(
                "Message {Id} already locked by another worker. Skipping.", message.Id);
            return;
        }

        try
        {
            await _publisher.PublishAsync(message.Type, message.Payload, ct);
        }
        finally
        {
            message.LockId = null; // always release
        }
    }

    // ── inbox: deserialize → dispatch via MediatR ─────
    private async Task ProcessInboxAsync(Message message, CancellationToken ct)
    {
        INotification notification = Deserialize(message.Type, message.Payload);
        await _mediator.Publish(notification, ct);
    }

    private async Task<bool> TryAcquireLockAsync(Guid messageId, string lockId)
    {
        int rows = await _db.Database.ExecuteSqlRawAsync(@"
            UPDATE ""Messages""
            SET    ""LockId"" = {0}
            WHERE  ""Id""          = {1}
              AND  ""Direction""   = 'Outbox'
              AND  ""LockId""      IS NULL
              AND  ""ProcessedAt"" IS NULL",
            lockId, messageId);

        return rows == 1;
    }

    private static INotification Deserialize(string type, string payload)
    {
        IReadOnlyDictionary<string, Type> typeMap = new Dictionary<string, Type>
        {
            [MessageTypes.Orders.Created]     = typeof(OrderCreatedEvent),
            [MessageTypes.Payments.Processed] = typeof(PaymentProcessedEvent),
            [MessageTypes.Stripe.PaymentSucceeded] = typeof(StripePaymentSucceededEvent),
        };

        if (!typeMap.TryGetValue(type, out Type? targetType))
        {
            throw new InvalidOperationException($"Unknown message type: {type}");
        }

        return (INotification)JsonSerializer.Deserialize(payload, targetType)!;
    }
}
