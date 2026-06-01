using MediatR;
using MusicShop.Application.Common;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;

namespace MusicShop.Application.UseCases.Shop.Orders.Queries.GetOrderHistory;

public sealed class GetOrderHistoryQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetOrderHistoryQuery, Result<PaginatedResult<OrderListItemDto>>>
{
    public async Task<Result<PaginatedResult<OrderListItemDto>>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        Guid userId = Guid.Parse(currentUserService.UserId!);

        (IReadOnlyList<Order> orders, int totalCount) = await orderRepository.GetPagedByUserIdAsync(
            userId, 
            request.Status, 
            request.Page, 
            request.Limit, 
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
            new PaginatedResult<OrderListItemDto>(orderListItemDtos, totalCount, request.Page, request.Limit));
    }
}

