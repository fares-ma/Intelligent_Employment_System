namespace Services.Abstractions.DTOs.Company;

/// <summary>
/// DTO for requesting admin transfer to another recruiter
/// Only current admin can initiate transfer
/// </summary>
public class AdminTransferDto
{
    /// <summary>
    /// ID of the recruiter to transfer admin role to
    /// Must be an existing recruiter in the company with Standard role
    /// </summary>
    public string NewAdminId { get; set; } = string.Empty;

    /// <summary>
    /// Optional reason for the admin transfer
    /// </summary>
    public string? Reason { get; set; }
}
