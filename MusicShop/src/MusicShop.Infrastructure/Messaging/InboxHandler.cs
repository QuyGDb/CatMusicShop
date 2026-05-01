using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Entities.Messaging;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Messaging;

public sealed class InboxHandler : IInboxHandler
{
    private readonly AppDbContext _db;
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<InboxHandler> _logger;

    public InboxHandler(
        AppDbContext db,
        IBackgroundJobClient jobs,
        ILogger<InboxHandler> logger)
    {
        _db    = db;
        _jobs  = jobs;
        _logger = logger;
    }

    public async Task HandleAsync(
        string messageId,
        string messageType,
        string payload,
        CancellationToken ct = default)
    {
        // ── idempotency check ──────────────────────────
        bool alreadyReceived = await _db.Messages
            .AnyAsync(m => m.Direction == MessageDirection.Inbox
                        && m.MessageId == messageId, ct);

        if (alreadyReceived)
        {
            _logger.LogInformation("InboxMessage {MessageId} already received. Skipping.", messageId);
            return;
        }

        // ── save to inbox ──────────────────────────────
        Message inbox = new()
        {
            Direction = MessageDirection.Inbox,
            MessageId = messageId,
            Type      = messageType,
            Payload   = payload
        };

        try
        {
            _db.Messages.Add(inbox);
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogInformation(
                "InboxMessage {MessageId} saved by concurrent request. Skipping.", messageId);
            return;
        }

        // ── enqueue processing ─────────────────────────
        _jobs.Enqueue<IMessageProcessor>(p => p.ProcessAsync(inbox.Id, default));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        string msg = ex.InnerException?.Message ?? "";
        return msg.Contains("23505") || msg.Contains("2601") || msg.Contains("2627");
    }
}
