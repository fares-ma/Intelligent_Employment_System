using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.Abstractions.TalentX.Models;

/// <summary>
/// Converts JSON values that may be either a number or a string into a string.
/// TalentX sends candidate_id as int (4423) or string GUID ("38b237e8-...").
/// </summary>
public class FlexibleStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt64().ToString(),
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

/// <summary>
/// Webhook body from TalentX when scoring completes or fails.
/// </summary>
public class TalentXWebhookPayload
{
  [JsonPropertyName("candidate_id")]
  [JsonConverter(typeof(FlexibleStringConverter))]
  public string CandidateId { get; set; } = string.Empty;

  [JsonPropertyName("job_id")]
  public int JobId { get; set; }

  [JsonPropertyName("status")]
  public string Status { get; set; } = string.Empty;

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

