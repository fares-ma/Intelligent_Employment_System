namespace Services.Abstractions.DTOs.Company;

/// <summary>
/// Response DTO for admin transfer operation
/// Confirms successful transfer of admin role
/// </summary>
public class AdminTransferResponseDto
{
    /// <summary>
    /// Confirmation message
    /// </summary>
    public string Message { get; set; } = "Admin role transferred successfully";

    /// <summary>
    /// Company ID where transfer occurred
    /// </summary>
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the previous admin
    /// </summary>
    public string PreviousAdminId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the new admin
    /// </summary>
    public string NewAdminId { get; set; } = string.Empty;

    /// <summary>
    /// When the transfer was completed
    /// </summary>
    public DateTime TransferedAt { get; set; }
}
