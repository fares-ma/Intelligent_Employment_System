using System.Text.Json;

namespace Services.TalentX;

internal static class TalentXJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    public static readonly JsonSerializerOptions WebhookDeserialize = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
