using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Entities.Messaging;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Messaging;

[AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]
[Queue("messages")]
public sealed class MessageProcessor : IMessageProcessor
{
    private readonly AppDbContext _db;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(
        AppDbContext db,
        IMessagePublisher publisher,
        ILogger<MessageProcessor> logger)
    {
        _db        = db;
        _publisher = publisher;
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
                message.LockId = null;
            }

            message.ProcessedAt = DateTime.UtcNow;
            message.Error       = null;

            _logger.LogInformation(
                "Message {Id} ({Type}) processed successfully.",
                messageId, message.Type);
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message;

            _logger.LogError(ex,
                "Message {Id} ({Type}) failed. RetryCount={Count}",
                messageId, message.Type, message.RetryCount);

            throw;
        }
        finally
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<bool> TryAcquireLockAsync(Guid messageId, string lockId)
    {
        int rows = await _db.Database.ExecuteSqlRawAsync(@"
            UPDATE ""Messages""
            SET    ""LockId"" = {0}
            WHERE  ""Id""          = {1}
              AND  ""LockId""      IS NULL
              AND  ""ProcessedAt"" IS NULL",
            lockId, messageId);

        return rows == 1;
    }
}
