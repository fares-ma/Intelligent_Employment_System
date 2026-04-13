namespace IES.api.Authentication;

public sealed class AnonymousApiAuthOptions
{
    /// <summary>Claim value for NameIdentifier when anonymous API mode is enabled.</summary>
    public string UserId { get; set; } = "00000000-0000-0000-0000-000000000001";
}
