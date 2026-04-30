using MusicShop.Application.Common.Models;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;

namespace MusicShop.Application.Common.Interfaces;

public interface IStripeService
{
    Task<Result<StripeCheckoutDto>> CreateCheckoutSessionAsync(
        Order order,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the webhook signature and returns the process result.
    /// </summary>
    Task<WebhookProcessResult> HandleWebhookAsync(string json, string signature, CancellationToken cancellationToken = default);

    Task<Result> RefundOrderAsync(string transactionCode, CancellationToken cancellationToken = default);
}
