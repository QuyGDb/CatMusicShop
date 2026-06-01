using Hangfire;
using System.Linq;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Domain.Entities.Messaging;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Infrastructure.Services;

public sealed class OutboxRecoveryJob(
    IRepository<OutboxMessage> outboxRepository,
    IBackgroundJobClient backgroundJobClient)
{
    public async Task RecoverUnprocessedMessagesAsync(CancellationToken cancellationToken)
    {
        DateTime cutoffTime = DateTime.UtcNow.AddMinutes(-5);
        
        int batchSize = 100;
        int maxEnqueuesPerRun = 1000; // Safety ceiling to prevent infinite loops
        int totalEnqueued = 0;

        while (totalEnqueued < maxEnqueuesPerRun)
        {
            IReadOnlyList<OutboxMessage> unprocessedMessages = await outboxRepository.ListAsync(
                predicate: message => (message.Status == "PENDING" || message.Status == "FAILED") && message.CreatedAt < cutoffTime,
                orderBy: query => query.OrderBy(message => message.CreatedAt),
                take: batchSize,
                cancellationToken: cancellationToken);

            if (unprocessedMessages.Count == 0)
            {
                break;
            }

            foreach (OutboxMessage message in unprocessedMessages)
            {
                backgroundJobClient.Enqueue<IOutboxProcessor>(processor =>
                    processor.ProcessAsync(message.Id, CancellationToken.None));
            }

            totalEnqueued += unprocessedMessages.Count;

            // Exit early if we fetched less than the full batch size (backlog is cleared)
            if (unprocessedMessages.Count < batchSize)
            {
                break;
            }
        }
    }
}

