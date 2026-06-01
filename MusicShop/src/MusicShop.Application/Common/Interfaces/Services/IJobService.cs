namespace MusicShop.Application.Common.Interfaces.Services;

public interface IJobService
{
    void EnqueueOutboxMessage(Guid messageId);
}
