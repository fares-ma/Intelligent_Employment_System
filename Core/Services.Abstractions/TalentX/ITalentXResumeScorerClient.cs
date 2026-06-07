using Services.Abstractions.TalentX.Models;

namespace Services.Abstractions.TalentX;

/// <summary>
/// Typed HTTP client for TalentX AI Resume Scorer API.
/// </summary>
public interface ITalentXResumeScorerClient
{
  Task<TalentXScoreAcceptedResponse> InitiateScoringAsync(
      TalentXScoreRequest request,
      string idempotencyKey,
      CancellationToken cancellationToken = default);

  Task<TalentXScoreResultResponse?> GetResultsAsync(
      int candidateId,
      int jobId,
      CancellationToken cancellationToken = default);

  Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
