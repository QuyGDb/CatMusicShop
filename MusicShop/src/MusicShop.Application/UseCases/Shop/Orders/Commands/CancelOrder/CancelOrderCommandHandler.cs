using MediatR;
using MusicShop.Application.Common.Interfaces;
using MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;
using MusicShop.Domain.Common;
using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;
using MusicShop.Domain.Errors;
using MusicShop.Domain.Interfaces;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUserService,
    IMediator mediator) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        // Security: Only Owner can cancel via this endpoint
        if (string.IsNullOrEmpty(currentUserService.UserId))
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        Guid userId = Guid.Parse(currentUserService.UserId);
        if (order.UserId != userId)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        // Forward to the unified status update logic
        // This will trigger the CancelOrderAction (stock restoration, refund, etc.)
        return await mediator.Send(new UpdateOrderStatusCommand(
            request.OrderId, 
            OrderStatus.Cancelled, 
            null), cancellationToken);
    }
}
