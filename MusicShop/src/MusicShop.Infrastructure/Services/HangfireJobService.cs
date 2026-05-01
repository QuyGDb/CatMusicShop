using Hangfire;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Infrastructure.Messaging;

namespace MusicShop.Infrastructure.Services;

public sealed class HangfireJobService(IBackgroundJobClient backgroundJobClient) : IJobService
{
    public void EnqueueMessageProcessing(Guid messageId)
    {
        backgroundJobClient.Enqueue<IMessageProcessor>(processor => processor.ProcessAsync(messageId, default));
    }
}
