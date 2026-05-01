namespace MusicShop.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string messageType, string jsonPayload, CancellationToken ct = default);
}
