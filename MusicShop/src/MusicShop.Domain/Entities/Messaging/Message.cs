using MusicShop.Domain.Common;

namespace MusicShop.Domain.Entities.Messaging;

public enum MessageDirection
{
    Outbox = 0,
    Inbox  = 1
}

public class Message : BaseEntity
{
    // Id, CreatedAt, UpdatedAt are inherited from BaseEntity

    // ─── DIRECTION ─────────────────────────────────────
    public MessageDirection Direction { get; set; }

    // ─── CONTENT ───────────────────────────────────────
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    // ─── LIFECYCLE ─────────────────────────────────────
    public DateTime? ProcessedAt { get; set; }

    // ─── ERROR HANDLING ────────────────────────────────
    public string? Error { get; set; }
    public int RetryCount { get; set; } = 0;

    // ─── OUTBOX ONLY ───────────────────────────────────
    public string? IdempotencyKey { get; set; }
    public string? LockId { get; set; }

    // ─── INBOX ONLY ────────────────────────────────────
    public string? MessageId { get; set; }
}
