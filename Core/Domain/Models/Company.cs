namespace Domain.Models;

/// <summary>
/// Company entity — employer organization with unique tax number.
/// </summary>
public class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Industry { get; set; }

    public string? Website { get; set; }

    /// <summary>
    /// Immutable after creation. Must be unique across all companies.
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Description { get; set; }

    public string? LogoPath { get; set; }

    public bool IsVerified { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Recruiter> Recruiters { get; set; } = new List<Recruiter>();
    public ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
}
