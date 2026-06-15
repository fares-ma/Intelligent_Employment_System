namespace Services.TalentX;

/// <summary>
/// Maps IES string candidate GUIDs to stable positive integers for TalentX API correlation.
/// The same GUID always yields the same int so webhooks and polling stay consistent.
/// </summary>
public static class TalentXIdMapper
{
    public static string BuildIdempotencyKey(string candidateId, int jobId, DateTime? timestampUtc = null)
    {
        var ts = (timestampUtc ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ss");
        return $"talentx-match-{candidateId}-{jobId}-{ts}";
    }
}
