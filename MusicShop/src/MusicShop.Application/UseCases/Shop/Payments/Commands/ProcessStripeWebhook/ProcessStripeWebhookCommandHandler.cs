using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.Common.Models;
using MusicShop.Domain.Common;

namespace MusicShop.Application.UseCases.Shop.Payments.Commands.ProcessStripeWebhook;

public sealed class ProcessStripeWebhookCommandHandler(
    IStripeService stripeService,
    IMediator mediator) : IRequestHandler<ProcessStripeWebhookCommand, Result>
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

        // Send the command directly — UpdateOrderStatusCommandHandler
        // already handles idempotency (fromStatus == targetStatus → Success)
        if (result.Command is not null)
        {
            return await mediator.Send(result.Command, cancellationToken);
        }

        return Result.Success();
    }
}

