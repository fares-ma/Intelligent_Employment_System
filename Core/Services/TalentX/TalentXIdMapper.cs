namespace Services.TalentX;

/// <summary>
/// Maps IES string candidate GUIDs to stable positive integers for TalentX API correlation.
/// The same GUID always yields the same int so webhooks and polling stay consistent.
/// </summary>
public static class TalentXIdMapper
{
    public static int ToTalentXCandidateId(string candidateId)
    {
        if (!Guid.TryParse(candidateId, out var guid))
            throw new ArgumentException("Candidate id must be a valid GUID.", nameof(candidateId));

        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        var hash = BitConverter.ToInt32(bytes[..4]);
        return Math.Abs(hash == 0 ? 1 : hash);
    }

    public static string BuildIdempotencyKey(int candidateId, int jobId, DateTime? timestampUtc = null)
    {
        var ts = (timestampUtc ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ss");
        return $"talentx-match-{candidateId}-{jobId}-{ts}";
    }
}
