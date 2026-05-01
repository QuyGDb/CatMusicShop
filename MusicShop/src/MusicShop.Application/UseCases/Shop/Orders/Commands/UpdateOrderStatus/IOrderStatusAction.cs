using MusicShop.Domain.Entities.Orders;
using MusicShop.Domain.Enums;

namespace MusicShop.Application.UseCases.Shop.Orders.Commands.UpdateOrderStatus;

public interface IOrderStatusAction
{
    bool CanHandle(OrderStatus from, OrderStatus to);
    Task ExecuteAsync(Order order, UpdateOrderStatusCommand request, CancellationToken ct);
}
