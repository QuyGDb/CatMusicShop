using System.Collections.Generic;
using MusicShop.Domain.Common;

using MusicShop.Domain.Errors;
using MusicShop.Domain.Enums;

namespace MusicShop.Domain.Entities.Orders;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }

    public string RecipientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? Note { get; set; }

    public Guid? CancelledBy { get; set; }
    public string? CancelReason { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }

    public Result TransitionTo(OrderStatus targetStatus)
    {
        if (Status == targetStatus) return Result.Success();

        bool isValid = (Status, targetStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            
            (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (OrderStatus.Shipped, OrderStatus.Cancelled) => true,
            
            (OrderStatus.Delivered, OrderStatus.Cancelled) => true,
            
            _ => false
        };

        if (!isValid)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        Status = targetStatus;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
