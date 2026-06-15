using Domain.Contracts;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Abstractions;
using Services.Abstractions.DTOs.Company;
using Shared.Configuration;

namespace Services;

/// <summary>
/// Service implementation for company management operations
/// Handles profile retrieval, updates, and admin role transfers
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IOptions<FileStorageSettings> _fileStorageSettings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IOptions<FileStorageSettings> fileStorageSettings,
        UserManager<ApplicationUser> userManager,
        ILogger<CompanyService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _fileStorageSettings = fileStorageSettings ?? throw new ArgumentNullException(nameof(fileStorageSettings));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all companies with active job post counts (paginated)
    /// </summary>
    public async Task<IEnumerable<CompanyProfileDto>> GetAllCompaniesAsync(int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Fetching companies - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

        var companies = await _unitOfWork.Companies.GetAllWithIncludesAsync();

        var result = companies
            .Where(c => c.IsActive)
            .Select(company =>
            {
                var admin = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
                return new CompanyProfileDto
                {
                    Id = company.Id.ToString(),
                    Name = company.Name,
                    TaxNumber = company.TaxNumber,
                    Industry = company.Industry,
                    Website = company.Website,
                    Description = company.Description,
                    LogoPath = company.LogoPath,
                    ActiveJobPostsCount = company.JobPosts.Count(jp => jp.IsActive && jp.DeletedAt == null),
                    RecruiterCount = company.Recruiters.Count,
                    AdminId = admin?.Id ?? string.Empty,
                    AdminName = admin is not null ? $"{admin.FirstName} {admin.LastName}" : string.Empty,
                    CreatedAt = company.CreatedAt
                };
            })
            .OrderByDescending(c => c.ActiveJobPostsCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("Retrieved {Count} companies", result.Count);
        return result;
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

        var company = await _unitOfWork.Companies.GetByIdWithIncludesAsync(id, tracking: false);
        if (company is null)
        {
            _logger.LogWarning("Company not found: {CompanyId}", id);
            throw new NotFoundException($"Company with ID '{id}' not found");
        }

        var adminRecruiter = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
        if (adminRecruiter is null && company.Recruiters.Count > 0)
        {
            _logger.LogError("Company {CompanyId} has recruiters but no admin role assigned", id);
            throw new NotFoundException("Company admin could not be resolved");
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
            LogoPath = company.LogoPath,
            ActiveJobPostsCount = activeJobPostsCount,
            RecruiterCount = recruiterCount,
            AdminId = adminRecruiter?.Id ?? string.Empty,
            AdminName = adminRecruiter is not null ? $"{adminRecruiter.FirstName} {adminRecruiter.LastName}" : string.Empty,
            CreatedAt = company.CreatedAt
        };

        _logger.LogInformation("Company profile retrieved successfully for company ID: {CompanyId}", id);
        return profile;
    }

    /// <inheritdoc />
    public async Task<CompanyProfileDto> CreateCompanyAsync(
        string? userId,
        CreateCompanyRequestDto request,
        Stream? brandAssetStream,
        string? brandAssetFileName)
    {
        _logger.LogInformation("Create company requested (user id: {UserId})", userId ?? "(none)");

        Recruiter? recruiterToLink = null;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            recruiterToLink = await _unitOfWork.Recruiters.GetByIdAsync(userId);
            if (recruiterToLink?.CompanyId is not null)
                throw new ConflictException("This account is already linked to a company.");
        }

        var name = request.Name?.Trim() ?? string.Empty;
        var industry = request.Industry?.Trim() ?? string.Empty;
        var taxNumber = request.TaxNumber?.Trim() ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();

        if (name.Length is < 2 or > 100)
            throw new BadRequestException("Company name must be between 2 and 100 characters");

        if (industry.Length is < 1 or > 100)
            throw new BadRequestException("Industry must be between 1 and 100 characters");

        if (taxNumber.Length > 50)
            throw new BadRequestException("Tax number must be at most 50 characters");

        if (description is not null && description.Length > 1000)
            throw new BadRequestException("Company description cannot exceed 1000 characters");

        if (website is not null)
        {
            if (!Uri.TryCreate(website, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new BadRequestException("Website must be a valid http or https URL");
            }
        }



        var company = new Company
        {
            Name = name,
            Industry = industry,
            Website = website,
            TaxNumber = Guid.NewGuid().ToString(),
            Description = description,
            LogoPath = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Companies.Create(company);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            if (brandAssetStream is not null && !string.IsNullOrWhiteSpace(brandAssetFileName))
            {
                var absolutePath = await _fileStorageService.SaveFileAsync(brandAssetFileName, brandAssetStream, "company-brand");
                company.LogoPath = ToRelativeStoragePath(absolutePath);
                company.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Companies.Update(company);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist brand asset for new company {CompanyId}", company.Id);
            try
            {
                _unitOfWork.Companies.Delete(company);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception cleanupEx)
            {
                _logger.LogError(cleanupEx, "Cleanup failed after brand upload failure for company {CompanyId}", company.Id);
            }

            throw;
        }

        if (recruiterToLink is not null)
        {
            recruiterToLink.CompanyId = company.Id;
            recruiterToLink.RecruiterRole = UserRole.Admin;
            _unitOfWork.Recruiters.Update(recruiterToLink);
            await _unitOfWork.SaveChangesAsync();

            if (!await _userManager.IsInRoleAsync(recruiterToLink, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(recruiterToLink, "Admin");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to assign Admin role after company create: {Errors}", errors);
                    throw new InvalidOperationException($"Failed to assign Admin role: {errors}");
                }
            }

            _logger.LogInformation("Company {CompanyId} created and linked to recruiter {UserId}", company.Id, userId);
        }
        else
        {
            _logger.LogInformation("Company {CompanyId} created (standalone, no recruiter linked)", company.Id);
        }

        return await GetCompanyProfileAsync(company.Id.ToString());
    }

    private string ToRelativeStoragePath(string absolutePath)
    {
        var baseFull = Path.GetFullPath(_fileStorageSettings.Value.BasePath);
        var full = Path.GetFullPath(absolutePath);
        if (!full.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
            return absolutePath;
        var rel = Path.GetRelativePath(baseFull, full);
        return rel.Replace('\\', '/');
    }

    public async Task<string> UpdateLogoAsync(string companyId, string userId, string fileName, Stream fileStream)
    {
        if (!int.TryParse(companyId, out var id))
        {
            throw new BadRequestException("Invalid company ID format");
        }

        var company = await _unitOfWork.Companies.GetByIdWithIncludesAsync(id, tracking: true);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID '{id}' not found");
        }

        var adminRecruiter = company.Recruiters.FirstOrDefault(r => r.RecruiterRole == UserRole.Admin);
        if (adminRecruiter?.Id != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update logo for company {CompanyId} without admin rights", userId, id);
            throw new ForbiddenException("Only the company admin can update the company logo");
        }

        if (!string.IsNullOrWhiteSpace(company.LogoPath))
        {
            await _fileStorageService.DeleteFileAsync(company.LogoPath);
        }

        var newPath = await _fileStorageService.SaveFileAsync(fileName, fileStream, "company-logos");
        company.LogoPath = ToRelativeStoragePath(newPath);

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync();

        return company.LogoPath;
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
        var company = await _unitOfWork.Companies.GetByIdWithIncludesAsync(id, tracking: true);
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
        var company = await _unitOfWork.Companies.GetByIdWithIncludesAsync(id, tracking: true);
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
            TransferredAt = DateTime.UtcNow
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
        var company = await _unitOfWork.Companies.GetByIdWithIncludesAsync(id, tracking: true);
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
