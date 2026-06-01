using MusicShop.Domain.Common;

namespace MusicShop.Domain.Entities.Messaging;

public class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING"; // PENDING / PUBLISHED / FAILED
}
