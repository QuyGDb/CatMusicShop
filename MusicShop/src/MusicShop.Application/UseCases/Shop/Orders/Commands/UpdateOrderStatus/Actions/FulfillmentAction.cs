using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus.Actions;

public sealed class FulfillmentAction : IOrderStatusAction
{
    public bool CanHandle(OrderStatus from, OrderStatus to) 
        => to == OrderStatus.Shipped;

    public Task ExecuteAsync(Order order, UpdateOrderStatusCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.TrackingNumber))
        {
            order.TrackingNumber = request.TrackingNumber;
        }
        
        return Task.CompletedTask;
    }
}
