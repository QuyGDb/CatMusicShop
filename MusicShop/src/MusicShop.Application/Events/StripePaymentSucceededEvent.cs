using MediatR;

namespace MusicShop.Application.Events;

public class StripePaymentSucceededEvent : INotification
{
    public Guid OrderId { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
}
