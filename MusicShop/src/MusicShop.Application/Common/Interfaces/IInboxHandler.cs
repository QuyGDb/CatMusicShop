namespace MusicShop.Application.Common.Interfaces;

public interface IInboxHandler
{
    Task HandleAsync(
        string messageId,
        string messageType,
        string payload,
        CancellationToken ct = default);
}
