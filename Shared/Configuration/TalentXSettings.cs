namespace Shared.Configuration;

/// <summary>
/// Configuration for TalentX AI Resume Scorer integration.
/// </summary>
public class TalentXSettings
{
    public const string SectionName = "TalentX";

    /// <summary>
    /// Base URL of the TalentX service (e.g. http://localhost:8008).
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:8008";

    /// <summary>
    /// When false, scoring initiation is skipped (useful for local dev without AI).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Optional shared secret sent by TalentX in X-Webhook-Secret header.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Applications in Processing longer than this are eligible for polling fallback.
    /// </summary>
    public int PollingFallbackAfterMinutes { get; set; } = 10;

    /// <summary>
    /// Maximum applications to poll per background cycle.
    /// </summary>
    public int PollingBatchSize { get; set; } = 20;

    public int HttpTimeoutSeconds { get; set; } = 120;
}
