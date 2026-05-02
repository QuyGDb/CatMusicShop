using Microsoft.AspNetCore.Mvc;
using MusicShop.Application.UseCases.Shop.Payments.Commands.ProcessStripeWebhook;
using MusicShop.Domain.Common;
using MediatR;
using MusicShop.API.Controllers.Base;

namespace MusicShop.API.Controllers;

[Route("api/v1/payments/stripe")]
public sealed class PaymentsController(IMediator mediator) : BaseApiController
{
    [HttpPost("webhook")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook([FromServices] ILogger<PaymentsController> logger)
    {
        Request.EnableBuffering();
        using StreamReader reader = new(Request.Body);
        string json = await reader.ReadToEndAsync();
        string? signature = Request.Headers["Stripe-Signature"];

        if (string.IsNullOrWhiteSpace(signature))
        {
            return BadRequest("Missing Stripe-Signature header");
        }

        ProcessStripeWebhookCommand command = new(json, signature);
        Result result = await mediator.Send(command);

        return HandleNoContentResult(result);
    }
}
