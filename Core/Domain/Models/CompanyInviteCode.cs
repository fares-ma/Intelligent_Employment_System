namespace Domain.Models;

public class CompanyInviteCode
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Code { get; set; } = string.Empty; // 6-char alphanumeric, unique
    public int MaxUses { get; set; } = 5;
    public int CurrentUses { get; set; } = 0;
    public DateTime? ExpiresAt { get; set; }
    /// <summary>
    /// ID of the recruiter who created this invite code (required FK, no empty default)
    /// </summary>
    public required string CreatedByRecruiterId { get; set; }
    public ApplicationUser CreatedByRecruiter { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Concurrency token to prevent race conditions during concurrent redeem operations
    /// </summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
