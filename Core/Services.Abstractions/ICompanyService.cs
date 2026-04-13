using Services.Abstractions.DTOs.Company;

namespace Services.Abstractions;

/// <summary>
/// Service interface for company management operations
/// Handles profile retrieval, updates, and admin transfers
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Get company profile information by ID
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <returns>CompanyProfileDto with all company details</returns>
    /// <exception cref="NotFoundException">Thrown when company not found</exception>
    Task<CompanyProfileDto> GetCompanyProfileAsync(string companyId);

    /// <summary>
    /// Create a new company (optional brand image). If <paramref name="userId"/> is a recruiter without a company, links them as admin.
    /// Otherwise creates a standalone company record (e.g. anonymous or candidate callers).
    /// </summary>
    Task<CompanyProfileDto> CreateCompanyAsync(string? userId, CreateCompanyRequestDto request, Stream? brandAssetStream, string? brandAssetFileName);

    /// <summary>
    /// Update company information (name, industry, website, description)
    /// Only admin of the company can update
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <param name="userId">Currently authenticated user ID (must be admin)</param>
    /// <param name="request">Update request with new company information</param>
    /// <returns>Updated CompanyProfileDto</returns>
    /// <exception cref="NotFoundException">Thrown when company not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the company admin</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails</exception>
    Task<CompanyProfileDto> UpdateCompanyAsync(string companyId, string userId, UpdateCompanyDto request);

    /// <summary>
    /// Transfer admin role to another recruiter in the company
    /// Only current admin can initiate transfer
    /// New admin must be a Standard recruiter in the same company
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <param name="currentAdminId">Current admin user ID</param>
    /// <param name="request">Transfer request with new admin ID</param>
    /// <returns>AdminTransferResponseDto confirming the transfer</returns>
    /// <exception cref="NotFoundException">Thrown when company or new admin recruiter not found</exception>
    /// <exception cref="ForbiddenException">Thrown when current user is not the admin</exception>
    /// <exception cref="BadRequestException">Thrown when validation fails (e.g., new admin is already admin, not in same company)</exception>
    Task<AdminTransferResponseDto> TransferAdminAsync(string companyId, string currentAdminId, AdminTransferDto request);

    /// <summary>
    /// Get all active invitations issued by this company
    /// Only admin can view company invitations
    /// </summary>
    /// <param name="companyId">Company identifier</param>
    /// <param name="userId">Currently authenticated user ID (must be admin)</param>
    /// <returns>Count of active invite codes</returns>
    /// <exception cref="ForbiddenException">Thrown when user is not the company admin</exception>
    Task<int> GetActiveInvitationsCountAsync(string companyId, string userId);
}
