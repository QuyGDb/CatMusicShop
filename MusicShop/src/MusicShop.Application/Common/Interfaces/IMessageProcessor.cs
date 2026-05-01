namespace MusicShop.Application.Common.Interfaces;

public interface IMessageProcessor
{
    Task ProcessAsync(Guid messageId, CancellationToken ct = default);
}
