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
    public string CreatedByRecruiterId { get; set; } = string.Empty;
    public ApplicationUser CreatedByRecruiter { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
