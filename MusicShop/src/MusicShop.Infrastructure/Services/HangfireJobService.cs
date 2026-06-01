using Hangfire;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;

namespace MusicShop.Infrastructure.Services;

public sealed class HangfireJobService(IBackgroundJobClient backgroundJobClient) : IJobService
{
    public void EnqueueOutboxMessage(Guid messageId)
    {
        backgroundJobClient.Enqueue<IOutboxProcessor>(processor => processor.ProcessAsync(messageId, default));
    }
}

