using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.Abstractions.TalentX.Models;

public class TalentXScoreResultResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("candidate_id")]
  public required string CandidateId { get; set; }

  [JsonPropertyName("job_id")]
  public int JobId { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("final_score")]
  public decimal? FinalScore { get; set; }

  [JsonPropertyName("fit_status")]
  public string? FitStatus { get; set; }

  [JsonPropertyName("raw_resume_data")]
  public JsonElement? RawResumeData { get; set; }

  [JsonPropertyName("matching_details")]
  public JsonElement? MatchingDetails { get; set; }

  [JsonPropertyName("error_message")]
  public string? ErrorMessage { get; set; }

  [JsonPropertyName("idempotency_key")]
  public string? IdempotencyKey { get; set; }
}
