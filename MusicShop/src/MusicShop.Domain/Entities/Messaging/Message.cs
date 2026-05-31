using MusicShop.Domain.Common;

namespace MusicShop.Domain.Entities.Messaging;

public class Message : BaseEntity
{
    // Id, CreatedAt, UpdatedAt are inherited from BaseEntity

    // ─── CONTENT ───────────────────────────────────────
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    // ─── LIFECYCLE ─────────────────────────────────────
    public DateTime? ProcessedAt { get; set; }

    // ─── ERROR HANDLING ────────────────────────────────
    public string? Error { get; set; }
    public int RetryCount { get; set; } = 0;

    // ─── CONCURRENCY ───────────────────────────────────
    public string? IdempotencyKey { get; set; }
    public string? LockId { get; set; }
}
