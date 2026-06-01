using MusicShop.Application.Common.Models;
using Microsoft.Extensions.Options;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Errors;
using Stripe;
using Stripe.Checkout;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;
using MusicShop.Domain.Enums;

namespace MusicShop.Infrastructure.Payments;

public sealed class StripeService(
    IOptions<StripeSettings> settings) : IStripeService
{
    private readonly StripeSettings _settings = settings.Value;

    public async Task<Result<StripeCheckoutDto>> CreateCheckoutSessionAsync(
        Order order,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            SessionService sessionService = new();
            List<SessionLineItemOptions> lineItems = order.OrderItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = ConvertToStripeAmount(item.PriceSnapshot),
                    Currency = _settings.Currency,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product?.Name ?? (string.IsNullOrWhiteSpace(item.ProductNameSnapshot) ? "Music Item" : item.ProductNameSnapshot),
                        Description = item.ProductNameSnapshot
                    },
                },
                Quantity = item.Quantity,
            }).ToList();

            string finalSuccessUrl = successUrl.Contains('?') 
                ? $"{successUrl}&order_id={order.Id}" 
                : $"{successUrl}?order_id={order.Id}";

            SessionCreateOptions options = new SessionCreateOptions
            {
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = finalSuccessUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", order.Id.ToString() }
                },
                ClientReferenceId = order.Id.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            Session session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);

            if (string.IsNullOrEmpty(session.Url))
            {
                return Result<StripeCheckoutDto>.Failure(PaymentErrors.CustomStripeError("Stripe failed to generate a checkout URL."));
            }

            return Result<StripeCheckoutDto>.Success(new StripeCheckoutDto(session.Id, session.Url));
        }
        catch (StripeException ex)
        {
            return Result<StripeCheckoutDto>.Failure(PaymentErrors.CustomStripeError(ex.Message));
        }
    }

    public Task<WebhookProcessResult> HandleWebhookAsync(
        string json,
        string signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Event stripeEvent = EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret, throwOnApiVersionMismatch: false);

            WebhookProcessResult result = stripeEvent.Type switch
            {
                "checkout.session.completed" => HandleCheckoutCompleted(stripeEvent),
                _ => WebhookProcessResult.Ignored(stripeEvent.Type)
            };
            
            return Task.FromResult(result);
        }
        catch (StripeException ex)
        {
            return Task.FromResult(WebhookProcessResult.Failure(PaymentErrors.CustomStripeError(ex.Message)));
        }
    }

    private WebhookProcessResult HandleCheckoutCompleted(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Session session)
        {
            return WebhookProcessResult.Failure(PaymentErrors.InvalidWebhookEvent);
        }

        if (!session.Metadata.TryGetValue("OrderId", out string? orderIdStr) || !Guid.TryParse(orderIdStr, out Guid orderId))
        {
            return WebhookProcessResult.Failure(PaymentErrors.InvalidWebhookEvent);
        }
        
        UpdateOrderStatusCommand command = new(orderId, OrderStatus.Confirmed, null, session.Id);
        return WebhookProcessResult.ShouldProcess(stripeEvent.Id, stripeEvent.Type, command);
    }

    private static long ConvertToStripeAmount(decimal price)
        => (long)Math.Round(price * 100, MidpointRounding.AwayFromZero);

    public async Task<Result> RefundOrderAsync(string transactionCode, CancellationToken cancellationToken = default)
    {
        try
        {
            SessionService sessionService = new();
            Session session = await sessionService.GetAsync(transactionCode, cancellationToken: cancellationToken);

            if (string.IsNullOrEmpty(session.PaymentIntentId))
            {
                return Result.Failure(PaymentErrors.CustomStripeError("No payment intent found for this session."));
            }

            RefundService refundService = new();
            RefundCreateOptions refundOptions = new()
            {
                PaymentIntent = session.PaymentIntentId
            };

            await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (StripeException ex)
        {
            return Result.Failure(PaymentErrors.CustomStripeError(ex.Message));
        }
    }
}

