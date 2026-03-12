namespace Services.Abstractions.DTOs.InviteCode;

public class InviteCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
