using MusicShop.Domain.Common;
using MediatR;

namespace MusicShop.Application.Common.Models;

public enum WebhookProcessStatus
{
    ShouldProcess,
    Ignored,
    Error
}

public sealed record WebhookProcessResult
{
    public WebhookProcessStatus Status { get; init; }
    public string? StripeEventId { get; init; }
    public string? EventType { get; init; }
    public IRequest<Result>? Command { get; init; }
    public Error? Error { get; init; }

    public static WebhookProcessResult ShouldProcess(string stripeEventId, string eventType, IRequest<Result> command) =>
        new() { Status = WebhookProcessStatus.ShouldProcess, StripeEventId = stripeEventId, EventType = eventType, Command = command };

    public static WebhookProcessResult Ignored(string eventType) =>
        new() { Status = WebhookProcessStatus.Ignored, EventType = eventType };

    public static WebhookProcessResult Failure(Error error) =>
        new() { Status = WebhookProcessStatus.Error, Error = error };

}
