using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

/// <summary>
/// Abstract base user entity — inherits IdentityUser. TPH discriminator: "UserType".
/// </summary>
public abstract class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? ProfilePicturePath { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
