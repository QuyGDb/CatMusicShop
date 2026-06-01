using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;

namespace MusicShop.Application.UseCases.Shop.Orders.Queries.GetAdminOrders;

public sealed class GetAdminOrdersQueryHandler(
    IOrderRepository orderRepository) : IRequestHandler<GetAdminOrdersQuery, Result<PaginatedResult<OrderListItemDto>>>
{
    public async Task<Result<PaginatedResult<OrderListItemDto>>> Handle(GetAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        (IReadOnlyList<Order> orders, int totalCount) = await orderRepository.GetPagedAllAsync(
            request.Status,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        List<OrderListItemDto> orderListItemDtos = orders.Select(order => new OrderListItemDto(
            order.Id,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.OrderItems.Count,
            order.RecipientName,
            order.Email
        )).ToList();

        return Result<PaginatedResult<OrderListItemDto>>.Success(
            new PaginatedResult<OrderListItemDto>(orderListItemDtos, totalCount, request.PageNumber, request.PageSize));
    }
}

