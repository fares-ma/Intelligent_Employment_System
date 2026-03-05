using Domain.Enums;

namespace Domain.Models;

/// <summary>
/// Employer representative — inherits ApplicationUser (TPH). Belongs to a Company.
/// </summary>
public class Recruiter : ApplicationUser
{
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public UserRole RecruiterRole { get; set; } = UserRole.Standard;

    // Navigation properties
    public ICollection<JobPost> CreatedJobPosts { get; set; } = new List<JobPost>();
}
