using System.Text.Json;
using MediatR;
using MusicShop.Application.Common.Constants;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Events;
using MusicShop.Domain.Entities.Messaging;
using MusicShop.Domain.Interfaces;

using Hangfire;

namespace MusicShop.Infrastructure.Services;

public sealed class OutboxProcessor(
    IRepository<OutboxMessage> outboxRepository,
    IUnitOfWork unitOfWork,
    IMediator mediator) : IOutboxProcessor
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAsync(Guid messageId, CancellationToken cancellationToken)
    {
        OutboxMessage? message = await outboxRepository.GetByIdAsync(messageId, cancellationToken);

        if (message == null)
        {
            return;
        }

        if (message.Status == "PUBLISHED")
        {
            return;
        }

        object? notification = message.EventType switch
        {
            MessageTypes.Orders.Created => JsonSerializer.Deserialize<OrderCreatedEvent>(message.Payload),
            _ => null
        };

        if (notification is null)
        {
            message.Status = "FAILED";
            outboxRepository.Update(message);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            await mediator.Publish(notification, cancellationToken);

            message.Status = "PUBLISHED";
            outboxRepository.Update(message);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            message.Status = "FAILED";
            outboxRepository.Update(message);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
            throw; // Rethrow to let Hangfire handle retry
        }
    }
}

