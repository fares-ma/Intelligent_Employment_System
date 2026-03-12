using Domain.Contracts;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Company;

namespace Services;

/// <summary>
/// Service implementation for company management operations
/// Handles profile retrieval, updates, and admin role transfers
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(IUnitOfWork unitOfWork, ILogger<CompanyService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get company profile information by ID
    /// </summary>
    public async Task<CompanyProfileDto> GetCompanyProfileAsync(string companyId)
    {
        if (!int.TryParse(companyId, out var id))
        {
            throw new BadRequestException("Invalid company ID format");
        }

        _logger.LogInformation("Fetching company profile for company ID: {CompanyId}", id);

        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            _logger.LogWarning("Company not found: {CompanyId}", id);
            throw new NotFoundException($"Company with ID '{id}' not found");
        }

        // Get admin recruiter (first recruiter with Admin role)
        var adminRecruiter = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
        if (adminRecruiter is null)
        {
            _logger.LogError("Admin recruiter not found for company: {CompanyId}", id);
            throw new NotFoundException($"Admin recruiter not found for company");
        }

        // Get count of active job posts
        var activeJobPostsCount = company.JobPosts.Count(jp => jp.DeletedAt == null);

        // Get count of recruiters
        var recruiterCount = company.Recruiters.Count;

        var profile = new CompanyProfileDto
        {
            Id = company.Id.ToString(),
            Name = company.Name,
            TaxNumber = company.TaxNumber,
            Industry = company.Industry,
            Website = company.Website,
            Description = company.Description,
            ActiveJobPostsCount = activeJobPostsCount,
            RecruiterCount = recruiterCount,
            AdminId = adminRecruiter.Id,
            AdminName = $"{adminRecruiter.FirstName} {adminRecruiter.LastName}",
            CreatedAt = company.CreatedAt
        };

        _logger.LogInformation("Company profile retrieved successfully for company ID: {CompanyId}", id);
        return profile;
    }

    /// <summary>
    /// Update company information
    /// Only admin can update
    /// </summary>
    public async Task<CompanyProfileDto> UpdateCompanyAsync(string companyId, string userId, UpdateCompanyDto request)
    {
        if (!int.TryParse(companyId, out var id))
        {
            throw new BadRequestException("Invalid company ID format");
        }

        _logger.LogInformation("Attempting to update company {CompanyId} by user {UserId}", id, userId);

        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 2 || request.Name.Length > 100)
        {
            throw new BadRequestException("Company name must be between 2 and 100 characters");
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 1000)
        {
            throw new BadRequestException("Company description cannot exceed 1000 characters");
        }

        // Get company
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            _logger.LogWarning("Company not found: {CompanyId}", id);
            throw new NotFoundException($"Company with ID '{id}' not found");
        }

        // Verify user is admin
        var adminRecruiter = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
        if (adminRecruiter?.Id != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update company {CompanyId} without admin rights", userId, id);
            throw new ForbiddenException("Only the company admin can update company information");
        }

        // Update company
        company.Name = request.Name;
        company.Industry = request.Industry;
        company.Website = request.Website;
        company.Description = request.Description;
        company.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Company {CompanyId} updated successfully by admin {UserId}", id, userId);

        // Return updated profile
        return await GetCompanyProfileAsync(companyId);
    }

    /// <summary>
    /// Transfer admin role to another recruiter
    /// Only current admin can initiate
    /// </summary>
    public async Task<AdminTransferResponseDto> TransferAdminAsync(string companyId, string currentAdminId, AdminTransferDto request)
    {
        if (!int.TryParse(companyId, out var id))
        {
            throw new BadRequestException("Invalid company ID format");
        }

        _logger.LogInformation("Attempting admin transfer for company {CompanyId} from {CurrentAdminId} to {NewAdminId}",
            id, currentAdminId, request.NewAdminId);

        if (string.IsNullOrWhiteSpace(request.NewAdminId))
        {
            throw new BadRequestException("New admin ID cannot be empty");
        }

        // Get company
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            _logger.LogWarning("Company not found: {CompanyId}", id);
            throw new NotFoundException($"Company with ID '{id}' not found");
        }

        // Get current admin recruiter
        var currentAdmin = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
        if (currentAdmin?.Id != currentAdminId)
        {
            _logger.LogWarning("User {UserId} is not admin of company {CompanyId}", currentAdminId, id);
            throw new ForbiddenException("Only the company admin can transfer admin role");
        }

        // Get new admin recruiter
        var newAdmin = company.Recruiters.FirstOrDefault(r => r.Id == request.NewAdminId);
        if (newAdmin is null)
        {
            _logger.LogWarning("Recruiter not found: {RecruiterId}", request.NewAdminId);
            throw new NotFoundException($"Recruiter with ID '{request.NewAdminId}' not found in this company");
        }

        // Validate new admin is not already admin
        if (newAdmin.RecruiterRole == UserRole.Admin)
        {
            throw new BadRequestException("Selected recruiter is already an admin. Cannot transfer to another admin.");
        }

        // Transfer admin role
        currentAdmin.RecruiterRole = UserRole.Standard;
        newAdmin.RecruiterRole = UserRole.Admin;

        _unitOfWork.Recruiters.Update(currentAdmin);
        _unitOfWork.Recruiters.Update(newAdmin);
        _unitOfWork.Companies.Update(company);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Admin role transferred successfully. Company {CompanyId}: {PreviousAdminId} -> {NewAdminId}",
            id, currentAdminId, request.NewAdminId);

        return new AdminTransferResponseDto
        {
            Message = "Admin role transferred successfully",
            CompanyId = id.ToString(),
            PreviousAdminId = currentAdminId,
            NewAdminId = newAdmin.Id,
            TransferedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get count of active invite codes for the company
    /// Only admin can view
    /// </summary>
    public async Task<int> GetActiveInvitationsCountAsync(string companyId, string userId)
    {
        if (!int.TryParse(companyId, out var id))
        {
            throw new BadRequestException("Invalid company ID format");
        }

        _logger.LogInformation("Fetching active invitations count for company {CompanyId} by user {UserId}", id, userId);

        // Get company
        var company = await _unitOfWork.Companies.GetByIdAsync(id);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID '{id}' not found");
        }

        // Verify user is admin
        var adminRecruiter = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
        if (adminRecruiter?.Id != userId)
        {
            _logger.LogWarning("User {UserId} attempted to view invitations for company {CompanyId} without admin rights", userId, id);
            throw new ForbiddenException("Only the company admin can view invitation codes");
        }

        var count = company.CompanyInviteCodes.Count(c => c.IsActive && c.ExpiresAt > DateTime.UtcNow);
        _logger.LogInformation("Active invitations count retrieved: {Count} for company {CompanyId}", count, id);
        return count;
    }
}

