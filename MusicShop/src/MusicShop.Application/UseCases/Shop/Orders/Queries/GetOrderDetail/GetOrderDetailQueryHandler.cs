using MediatR;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Application.DTOs.Shop;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Errors;

namespace MusicShop.Application.UseCases.Shop.Orders.Queries.GetOrderDetail;

public sealed class GetOrderDetailQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetOrderDetailQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<OrderDetailDto>.Failure(OrderErrors.NotFound);
        }

        // Authorization check: User must be Admin or the owner of the order
        Guid userId = Guid.Parse(currentUserService.UserId!);
        if (order.UserId != userId && !currentUserService.IsInRole("admin"))
        {
            // We return NotFound to avoid leaking information about order existence
            return Result<OrderDetailDto>.Failure(OrderErrors.NotFound);
        }

        OrderDetailDto orderDetailDto = new OrderDetailDto(
            order.Id,
            order.Status,
            order.RecipientName,
            order.Email,
            order.Phone,
            order.ShippingAddress,
            order.Note,
            order.TotalAmount,
            order.TrackingNumber,
            order.CreatedAt,
            order.OrderItems.Select(orderItem => new OrderItemDetailDto(
                orderItem.Id,
                orderItem.Quantity,
                orderItem.PriceSnapshot,
                orderItem.PriceSnapshot * orderItem.Quantity,
                orderItem.ProductId,
                orderItem.Product.Name,
                orderItem.Product.CoverUrl
            )).ToList(),
            order.Payment != null ? new PaymentDetailDto(
                order.Payment.Method,
                order.Payment.Status,
                order.Payment.PaidAt,
                order.Payment.TransactionCode
            ) : null);

        return Result<OrderDetailDto>.Success(orderDetailDto);
    }
}

