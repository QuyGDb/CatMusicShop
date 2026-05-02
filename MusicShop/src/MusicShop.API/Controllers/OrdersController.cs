using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicShop.API.Controllers.Base;
using MusicShop.API.Infrastructure;
using MusicShop.Application.Common;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;
using MusicShop.Application.UseCases.Shop.Orders.Queries.GetAdminOrders;
using MusicShop.Application.UseCases.Shop.Orders.Commands.CancelOrder;
using MusicShop.Application.UseCases.Shop.Orders.Commands.CreateOrder;
using MusicShop.Application.UseCases.Shop.Orders.Queries.GetOrderDetail;
using MusicShop.Application.UseCases.Shop.Orders.Queries.GetOrderHistory;
using MusicShop.Domain.Common;

namespace MusicShop.API.Controllers;

[Authorize]
public sealed class OrdersController(IMediator mediator) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderListItemDto>>>> GetOrderHistory([FromQuery] GetOrderHistoryQuery query)
    {
        Result<PaginatedResult<OrderListItemDto>> result = await mediator.Send(query);
        return HandlePaginatedResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetOrderDetail([FromRoute] Guid id)
    {
        Result<OrderDetailDto> result = await mediator.Send(new GetOrderDetailQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateOrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CreateOrderResponse>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        Result<CreateOrderResponse> result = await mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetOrderDetail), response => new { id = response.Order.Id });
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
    {
        Result result = await mediator.Send(new CancelOrderCommand(id));
        return HandleNoContentResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("/api/v1/admin/orders")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OrderListItemDto>>>> GetAdminOrders([FromQuery] GetAdminOrdersQuery query)
    {
        Result<PaginatedResult<OrderListItemDto>> result = await mediator.Send(query);
        return HandlePaginatedResult(result);
    }

    [Authorize(Roles = "admin")]
    [HttpPatch("/api/v1/admin/orders/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        Result result = await mediator.Send(new UpdateOrderStatusCommand(id, request.Status, request.TrackingNumber));
        return HandleNoContentResult(result);
    }
}
