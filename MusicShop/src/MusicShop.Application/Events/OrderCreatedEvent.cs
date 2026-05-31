using MediatR;

namespace MusicShop.Application.Events;

public class OrderCreatedEvent : INotification
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; } 
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
