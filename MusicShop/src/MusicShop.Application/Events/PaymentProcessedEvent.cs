using MediatR;

namespace MusicShop.Application.Events;

public class PaymentProcessedEvent : INotification
{
    public Guid OrderId { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
