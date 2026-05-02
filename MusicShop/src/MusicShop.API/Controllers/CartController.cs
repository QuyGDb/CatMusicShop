using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers;
using MusicShop.API.Infrastructure;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Application.UseCases.Shop.Cart.Commands.AddToCart;
using MusicShop.Application.UseCases.Shop.Cart.Commands.ClearCart;
using MusicShop.Application.UseCases.Shop.Cart.Commands.RemoveFromCart;
using MusicShop.Application.UseCases.Shop.Cart.Commands.UpdateCartItem;
using MusicShop.Application.UseCases.Shop.Cart.Queries.GetCart;
using MusicShop.Domain.Common;
using MusicShop.API.Controllers.Base;

namespace MusicShop.API.Controllers;

[Authorize]
public sealed class CartController(
    IMediator mediator,
    ICurrentUserService currentUserService) : BaseApiController
{
    private Guid GetUserId() => Guid.Parse(currentUserService.UserId!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
    {
        var result = await mediator.Send(new GetCartQuery(GetUserId()));
        return HandleResult(result);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> AddToCart([FromBody] AddToCartRequest request)
    {
        var result = await mediator.Send(new AddToCartCommand(
            GetUserId(),
            request.ProductId,
            request.Quantity));

        return HandleResult(result);
    }

    [HttpPut("items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCartItem(Guid id, [FromBody] UpdateCartItemRequest request)
    {
        Result result = await mediator.Send(new UpdateCartItemCommand(
            GetUserId(),
            id,
            request.Quantity));

        return HandleNoContentResult(result);
    }

    [HttpDelete("items/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(Guid id)
    {
        Result result = await mediator.Send(new RemoveFromCartCommand(GetUserId(), id));
        return HandleNoContentResult(result);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart()
    {
        Result result = await mediator.Send(new ClearCartCommand());
        return HandleNoContentResult(result);
    }
}



