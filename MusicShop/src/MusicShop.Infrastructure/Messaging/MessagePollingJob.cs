using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Domain.Entities.Messaging;
using MusicShop.Infrastructure.Persistence;

namespace MusicShop.Infrastructure.Messaging;

public sealed class MessagePollingJob
{
    private readonly AppDbContext _db;
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<MessagePollingJob> _logger;

    public MessagePollingJob(
        AppDbContext db,
        IBackgroundJobClient jobs,
        ILogger<MessagePollingJob> logger)
    {
        _db     = db;
        _jobs   = jobs;
        _logger = logger;
    }

    public async Task PollAsync(CancellationToken ct = default)
    {
        // ── release orphaned locks (stuck > 10 minutes) ──
        DateTime stuckCutoff = DateTime.UtcNow.AddMinutes(-10);

        await _db.Database.ExecuteSqlRawAsync(@"
            UPDATE ""Messages""
            SET    ""LockId"" = NULL
            WHERE  ""LockId""      IS NOT NULL
              AND  ""ProcessedAt"" IS NULL
              AND  ""CreatedAt""   < {0}",
            stuckCutoff);

        // ── find unprocessed messages ──────────────────
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-2);

        List<Message> unprocessed = await _db.Messages
            .Where(message => message.ProcessedAt == null
                     && message.LockId      == null
                     && message.CreatedAt    < cutoff)
            .OrderBy(message => message.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

        if (!unprocessed.Any())
        {
            return;
        }

        _logger.LogInformation("Polling found {Count} unprocessed messages.", unprocessed.Count);

        foreach (Message message in unprocessed)
        {
            _jobs.Enqueue<IMessageProcessor>(processor => processor.ProcessAsync(message.Id, default));
        }
    }
}
