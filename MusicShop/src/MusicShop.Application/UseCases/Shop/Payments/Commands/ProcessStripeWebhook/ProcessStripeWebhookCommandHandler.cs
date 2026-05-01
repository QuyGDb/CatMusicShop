using MediatR;
using MusicShop.Application.Common.Constants;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.Common.Models;
using MusicShop.Domain.Common;
using System.Text.Json;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;
using MusicShop.Application.Events;

namespace MusicShop.Application.UseCases.Shop.Payments.Commands.ProcessStripeWebhook;

public sealed class ProcessStripeWebhookCommandHandler(
    IStripeService stripeService,
    IInboxHandler inboxHandler) : IRequestHandler<ProcessStripeWebhookCommand, Result>
{
    public async Task<Result> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        WebhookProcessResult result = await stripeService.HandleWebhookAsync(request.Json, request.Signature, cancellationToken);

        if (result.Status == WebhookProcessStatus.Ignored)
        {
            return Result.Success();
        }

        if (result.Status == WebhookProcessStatus.Error)
        {
            return Result.Failure(result.Error!);
        }

        // 1. Extract info from the command if it's an UpdateOrderStatusCommand
        if (result.Command is UpdateOrderStatusCommand updateCommand)
        {
            StripePaymentSucceededEvent @event = new()
            {
                OrderId = updateCommand.OrderId,
                StripeSessionId = updateCommand.TransactionCode ?? string.Empty
            };

            // 2. Save to Inbox for reliable processing
            await inboxHandler.HandleAsync(
                messageId: result.StripeEventId!,
                messageType: MessageTypes.Stripe.PaymentSucceeded,
                payload: JsonSerializer.Serialize(@event),
                ct: cancellationToken);
        }

        return Result.Success();
    }
}
