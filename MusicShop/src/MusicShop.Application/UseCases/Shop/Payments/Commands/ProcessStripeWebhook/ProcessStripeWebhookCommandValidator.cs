using FluentValidation;

namespace MusicShop.Application.UseCases.Shop.Payments.Commands.ProcessStripeWebhook;

public sealed class ProcessStripeWebhookCommandValidator : AbstractValidator<ProcessStripeWebhookCommand>
{
    public ProcessStripeWebhookCommandValidator()
    {
        RuleFor(x => x.Json)
            .NotEmpty().WithMessage("Webhook payload is required.");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Stripe signature is required.");
    }
}
