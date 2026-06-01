namespace MusicShop.Application.Common.Interfaces.Services;

public interface IOutboxProcessor
{
    Task ProcessAsync(Guid messageId, CancellationToken cancellationToken);
}
