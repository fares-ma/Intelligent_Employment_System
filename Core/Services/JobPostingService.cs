using System.Text.Json;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.JobPosting;
using Shared.Pagination;

namespace Services;

/// <summary>
/// Service for managing job postings
/// </summary>
public class JobPostingService : IJobPostingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JobPostingService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public JobPostingService(IUnitOfWork unitOfWork, ILogger<JobPostingService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<JobPostingDto>> GetAllActiveJobPostingsAsync(int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving active job postings - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
        ValidatePagination(pageNumber, pageSize);

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _unitOfWork.JobPosts.GetPublishedJobsAsync(pagination);
        return result.Items.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<JobPostingDto>> GetCompanyJobPostingsAsync(int companyId, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Retrieving job postings for company {CompanyId}", companyId);
        ValidatePagination(pageNumber, pageSize);

        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company is null)
            throw new ArgumentException($"Company with ID {companyId} not found");

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _unitOfWork.JobPosts.GetCompanyJobsAsync(companyId, pagination);
        return result.Items.Select(MapToDto).ToList();
    }

    public async Task<JobPostingDto> GetJobPostingAsync(int jobPostingId)
    {
        var jobPost = await _unitOfWork.JobPosts.GetByIdWithSkillsAsync(jobPostingId);
        if (jobPost is null)
            throw new ArgumentException($"Job posting with ID {jobPostingId} not found");

        return MapToDto(jobPost);
    }

    public async Task<JobPostingDto> CreateJobPostingAsync(int companyId, string recruiterId, CreateJobPostingDto request)
    {
        _logger.LogInformation("Creating new job posting for company {CompanyId}", companyId);

        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company is null)
            throw new ArgumentException($"Company with ID {companyId} not found");

        var recruiter = await _unitOfWork.Recruiters.GetWithCompanyAsync(recruiterId);
        if (recruiter is null)
            throw new ArgumentException("Recruiter not found");

        if (recruiter.CompanyId != companyId)
            throw new UnauthorizedAccessException("Recruiter does not belong to this company");

        // Parse employment type
        var jobType = ParseEmploymentType(request.EmploymentType);

        // Auto-generate title if not provided
        var title = !string.IsNullOrWhiteSpace(request.Title)
            ? request.Title
            : BuildAutoTitle(request);

        // Auto-generate description if not provided
        var description = !string.IsNullOrWhiteSpace(request.Description)
            ? request.Description
            : BuildAutoDescription(request);

        var jobPost = new JobPost
        {
            Title = title,
            Description = description,
            CompanyId = companyId,
            CreatedByRecruiterId = recruiterId,
            JobType = jobType,
            ExpiryDate = request.ApplicationDeadline,
            Location = request.Location,
            IsPublished = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            JobPostSkills = new List<JobPostSkill>(),

            // New fields
            Department = request.Department,
            GPA = request.GPA,
            GPAPriority = request.GPAPriority,
            ExperienceMinYears = request.ExperienceMinYears,
            ExperienceMaxYears = request.ExperienceMaxYears,
            ExperiencePriority = request.ExperiencePriority,
            DegreesJson = request.Degrees.Any()
                ? JsonSerializer.Serialize(request.Degrees, _jsonOptions)
                : null,
            RolesJson = request.Roles.Any()
                ? JsonSerializer.Serialize(request.Roles, _jsonOptions)
                : null,
            SkillsJson = request.Skills.Any()
                ? JsonSerializer.Serialize(request.Skills, _jsonOptions)
                : null
        };

        // Add skills by ID (legacy) or by name (new format)
        if (request.Skills.Any())
        {
            foreach (var skillDto in request.Skills)
            {
                var skill = await GetOrCreateSkillAsync(skillDto.SkillName);
                if (skill is not null)
                {
                    jobPost.JobPostSkills.Add(new JobPostSkill
                    {
                        SkillId = skill.Id,
                        IsRequired = !string.Equals(skillDto.SkillPriority, "None", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
        }
        else if (request.RequiredSkillIds.Any())
        {
            foreach (var skillId in request.RequiredSkillIds)
            {
                var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
                if (skill is not null)
                {
                    jobPost.JobPostSkills.Add(new JobPostSkill { SkillId = skillId });
                }
            }
        }

        _unitOfWork.JobPosts.Create(jobPost);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job posting created with ID {JobPostingId}", jobPost.Id);
        return MapToDto(jobPost);
    }

    public async Task<JobPostingDto> UpdateJobPostingAsync(int jobPostingId, string recruiterId, UpdateJobPostingDto request)
    {
        _logger.LogInformation("Updating job posting {JobPostingId}", jobPostingId);

        var jobPost = await _unitOfWork.JobPosts.GetByIdWithSkillsAsync(jobPostingId);
        if (jobPost is null)
            throw new ArgumentException($"Job posting not found");

        if (jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to update this job posting");

        if (!string.IsNullOrWhiteSpace(request.Title))
            jobPost.Title = request.Title;
        if (!string.IsNullOrWhiteSpace(request.Description))
            jobPost.Description = request.Description;

        jobPost.ExpiryDate = request.ApplicationDeadline;
        if (request.IsActive.HasValue)
            jobPost.IsActive = request.IsActive.Value;
        jobPost.UpdatedAt = DateTime.UtcNow;

        // Update required skills
        jobPost.JobPostSkills ??= new List<JobPostSkill>();
        jobPost.JobPostSkills.Clear();
        if (request.RequiredSkillIds.Any())
        {
            foreach (var skillId in request.RequiredSkillIds)
            {
                var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
                if (skill != null)
                {
                    jobPost.JobPostSkills.Add(new JobPostSkill { JobPostId = jobPost.Id, SkillId = skillId });
                }
            }
        }

        _unitOfWork.JobPosts.Update(jobPost);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(jobPost);
    }

    public async Task DeleteJobPostingAsync(int jobPostingId, string recruiterId)
    {
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(jobPostingId);
        if (jobPost is null)
            throw new ArgumentException("Job posting not found");

        if (jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to delete this job posting");

        jobPost.IsDeleted = true;
        jobPost.Status = "CLOSED";
        jobPost.DeletedAt = DateTime.UtcNow;
        jobPost.DeletedBy = recruiterId;

        _unitOfWork.JobPosts.Update(jobPost);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job posting {JobPostingId} marked as deleted/closed", jobPostingId);
    }

    public async Task<IEnumerable<JobPostingDto>> SearchJobPostingsAsync(string searchTerm, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllActiveJobPostingsAsync(pageNumber, pageSize);

        ValidatePagination(pageNumber, pageSize);
        var term = searchTerm.ToLower();

        var jobPosts = await _unitOfWork.JobPosts.FindAsync(
            jp => jp.IsActive && !jp.IsDeleted && jp.Status == "ACTIVE" && (
                jp.Title.ToLower().Contains(term) ||
                jp.Description.ToLower().Contains(term)
            )
        );

        return jobPosts
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<IEnumerable<JobPostingDto>> GetJobPostingsBySkillAsync(int skillId, int pageNumber = 1, int pageSize = 20)
    {
        ValidatePagination(pageNumber, pageSize);

        var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
        if (skill is null)
            throw new ArgumentException("Skill not found");

        var jobPosts = await _unitOfWork.JobPosts.FindAsync(
            jp => jp.IsActive && !jp.IsDeleted && jp.Status == "ACTIVE" && jp.JobPostSkills.Any(jps => jps.SkillId == skillId)
        );

        return jobPosts
            .OrderByDescending(j => j.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<IEnumerable<JobPostingDto>> GetJobPostingsByTypeAsync(string employmentType, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(employmentType))
            throw new ArgumentException("Employment type is required");

        ValidatePagination(pageNumber, pageSize);

        var jobType = ParseEmploymentType(employmentType);

        var jobPosts = await _unitOfWork.JobPosts.FindAsync(
            jp => jp.IsActive && !jp.IsDeleted && jp.Status == "ACTIVE" && jp.JobType == jobType
        );

        return jobPosts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
    }

    public async Task<PagedResult<Services.Abstractions.DTOs.JobPosting.JobApplicantDto>> GetJobApplicantsAsync(int jobPostId, string recruiterId, PaginationParams pagination, Domain.Enums.ApplicationStatus? status, string? sortBy)
    {
        ValidatePagination(pagination.PageNumber, pagination.PageSize);

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(jobPostId);
        if (jobPost == null)
            throw new ArgumentException("Job posting not found");

        if (jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to access applicants for this job posting");

        // Retrieve job applications
        var applications = await _unitOfWork.JobApplications.FindAsync(ja => ja.JobPostId == jobPostId);

        if (status.HasValue)
        {
            applications = applications.Where(a => a.Status == status.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            applications = sortBy.ToLower() switch
            {
                "date" => applications.OrderByDescending(a => a.AppliedAt),
                "score" => applications.OrderByDescending(a => a.MatchScore),
                "status" => applications.OrderBy(a => a.Status),
                _ => applications.OrderByDescending(a => a.AppliedAt)
            };
        }
        else
        {
            applications = applications.OrderByDescending(a => a.AppliedAt);
        }

        var totalCount = applications.Count();

        var pagedApps = applications
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var dtos = new List<Services.Abstractions.DTOs.JobPosting.JobApplicantDto>();
        foreach (var app in pagedApps)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(app.CandidateId);
            
            dtos.Add(new Services.Abstractions.DTOs.JobPosting.JobApplicantDto
            {
                CandidateId = candidate?.Id ?? string.Empty,
                FullName = candidate != null ? $"{candidate.FirstName} {candidate.LastName}".Trim() : "Unknown",
                Email = candidate?.Email ?? "Unknown",
                CandidateProfilePictureUrl = candidate?.ProfilePicturePath,
                ApplicationStatus = app.Status.ToString(),
                CurrentStage = app.Status.ToString(),
                ApplicationDate = app.AppliedAt,
                MatchScore = app.MatchScore
            });
        }

        return new PagedResult<Services.Abstractions.DTOs.JobPosting.JobApplicantDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    // ── Private helpers ──

    private static JobPostingDto MapToDto(JobPost jobPost)
    {
        return new JobPostingDto
        {
            Id = jobPost.Id,
            Title = jobPost.Title,
            Description = jobPost.Description,
            Requirements = "",
            SalaryRange = jobPost.SalaryMin.HasValue && jobPost.SalaryMax.HasValue
                ? $"{jobPost.SalaryMin}-{jobPost.SalaryMax} {jobPost.Currency}"
                : null,
            Location = jobPost.Location ?? "",
            EmploymentType = jobPost.JobType.ToString(),
            CompanyId = jobPost.CompanyId,
            CompanyName = jobPost.Company?.Name ?? string.Empty,
            IsActive = jobPost.IsActive,
            ApplicationDeadline = jobPost.ExpiryDate,
            RequiredSkills = jobPost.JobPostSkills?.Select(jps => jps.Skill?.Name ?? "").Where(n => n != "").ToList() ?? new(),
            ApplicationCount = jobPost.JobApplications?.Count ?? 0,
            CreatedAt = jobPost.CreatedAt,
            UpdatedAt = jobPost.UpdatedAt ?? DateTime.UtcNow,

            // New fields
            Department = jobPost.Department,
            GPA = jobPost.GPA,
            GPAPriority = jobPost.GPAPriority,
            ExperienceMinYears = jobPost.ExperienceMinYears,
            ExperienceMaxYears = jobPost.ExperienceMaxYears,
            ExperiencePriority = jobPost.ExperiencePriority,
            Degrees = DeserializeJson<List<JobDegreeDto>>(jobPost.DegreesJson) ?? new(),
            Roles = DeserializeJson<List<JobRoleDto>>(jobPost.RolesJson) ?? new(),
            Skills = DeserializeJson<List<JobSkillDto>>(jobPost.SkillsJson) ?? new()
        };
    }

    private async Task<Skill?> GetOrCreateSkillAsync(string skillName)
    {
        if (string.IsNullOrWhiteSpace(skillName))
            return null;

        var normalized = skillName.Trim().ToLower();

        // Try to find existing skill
        var existing = await _unitOfWork.Skills.FindAsync(s => s.Name.ToLower() == normalized);
        var skill = existing.FirstOrDefault();

        if (skill is not null)
            return skill;

        // Create new skill
        skill = new Skill { Name = skillName.Trim() };
        _unitOfWork.Skills.Create(skill);
        await _unitOfWork.SaveChangesAsync();

        return skill;
    }

    private static JobType ParseEmploymentType(string? employmentType)
    {
        if (string.IsNullOrWhiteSpace(employmentType))
            return JobType.FullTime;

        return employmentType.Trim().ToLower() switch
        {
            "fulltime" or "full-time" or "full_time" => JobType.FullTime,
            "parttime" or "part-time" or "part_time" => JobType.PartTime,
            "contract" => JobType.Contract,
            "internship" => JobType.Internship,
            _ => Enum.TryParse<JobType>(employmentType, true, out var parsed) ? parsed : JobType.FullTime
        };
    }

    private static string BuildAutoTitle(CreateJobPostingDto request)
    {
        // Use Department + EmploymentType as a clean title
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Department))
            parts.Add(request.Department);
        if (!string.IsNullOrWhiteSpace(request.EmploymentType))
            parts.Add(request.EmploymentType);

        if (parts.Any())
            return string.Join(" - ", parts);

        // Fallback to first role name if no department
        if (request.Roles.Any())
            return request.Roles.First().RoleName;

        return "New Job Posting";
    }

    private static string BuildAutoDescription(CreateJobPostingDto request)
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.Department))
            lines.Add($"Department: {request.Department}");
        if (!string.IsNullOrWhiteSpace(request.EmploymentType))
            lines.Add($"Employment Type: {request.EmploymentType}");
        if (request.ExperienceMinYears.HasValue || request.ExperienceMaxYears.HasValue)
            lines.Add($"Experience: {request.ExperienceMinYears ?? 0}-{request.ExperienceMaxYears ?? 0} years");
        if (request.GPA.HasValue && request.GPA > 0)
            lines.Add($"Minimum GPA: {request.GPA}");
        if (request.Degrees.Any())
            lines.Add($"Required Degrees: {string.Join(", ", request.Degrees.Select(d => d.DegreeName))}");
        if (request.Roles.Any())
            lines.Add($"Roles: {string.Join(", ", request.Roles.Select(r => r.RoleName))}");
        if (request.Skills.Any())
            lines.Add($"Skills: {string.Join(", ", request.Skills.Select(s => s.SkillName))}");

        return lines.Any() ? string.Join("\n", lines) : "Job posting";
    }

    private static T? DeserializeJson<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100");
    }
}
