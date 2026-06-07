namespace Domain.Models;

/// <summary>
/// Stores processed webhook idempotency keys so TalentX retries do not double-apply scores.
/// </summary>
public class ProcessedIdempotencyKey
{
    public long Id { get; set; }

    /// <summary>
    /// Value from X-Idempotency-Key header or payload idempotency_key field.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    public int? JobApplicationId { get; set; }

    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;

    public string Source { get; set; } = "TalentXWebhook";
}
