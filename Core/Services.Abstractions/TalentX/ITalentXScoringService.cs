using Services.Abstractions.TalentX.Models;

namespace Services.Abstractions.TalentX;

/// <summary>
/// Orchestrates async TalentX scoring: initiation, webhook processing, and polling fallback.
/// </summary>
public interface ITalentXScoringService
{
  Task InitiateScoringForApplicationAsync(int jobApplicationId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Processes webhook payload with idempotency. Returns true when newly processed, false when duplicate.
  /// </summary>
  Task<bool> ProcessWebhookAsync(
      TalentXWebhookPayload payload,
      string idempotencyKey,
      CancellationToken cancellationToken = default);

  /// <summary>
  /// Polling fallback for applications stuck in Processing.
  /// </summary>
  Task<bool> PollAndApplyResultAsync(int jobApplicationId, CancellationToken cancellationToken = default);
}
