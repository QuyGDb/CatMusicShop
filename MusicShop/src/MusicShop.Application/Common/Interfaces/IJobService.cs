namespace MusicShop.Application.Common.Interfaces;

public interface IJobService
{
    void EnqueueMessageProcessing(Guid messageId);
}
