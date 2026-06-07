using System.Text.Json.Serialization;

namespace Services.Abstractions.TalentX.Models;

public class TalentXScoreAcceptedResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("message")]
  public string? Message { get; set; }

  [JsonPropertyName("candidate_id")]
  public int CandidateId { get; set; }

  [JsonPropertyName("job_id")]
  public int JobId { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }
}
