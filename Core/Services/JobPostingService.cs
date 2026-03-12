using Domain.Contracts;
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
        ValidateCreateJobPostingInput(request);

        var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company is null)
            throw new ArgumentException($"Company with ID {companyId} not found");

        var recruiter = await _unitOfWork.Recruiters.GetWithCompanyAsync(recruiterId);
        if (recruiter is null)
            throw new ArgumentException("Recruiter not found");

        if (recruiter.CompanyId != companyId)
            throw new UnauthorizedAccessException("Recruiter does not belong to this company");

        var jobPost = new JobPost
        {
            Title = request.Title,
            Description = request.Description,
            CompanyId = companyId,
            CreatedByRecruiterId = recruiterId,
            ExpiryDate = request.ApplicationDeadline,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            JobPostSkills = new List<JobPostSkill>()
        };

        // Add required skills
        if (request.RequiredSkillIds.Any())
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
        ValidateUpdateJobPostingInput(request);

        var jobPost = await _unitOfWork.JobPosts.GetByIdWithSkillsAsync(jobPostingId);
        if (jobPost is null)
            throw new ArgumentException($"Job posting not found");

        if (jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("You don't have permission to update this job posting");

        jobPost.Title = request.Title;
        jobPost.Description = request.Description;
        jobPost.ExpiryDate = request.ApplicationDeadline;
        jobPost.IsActive = request.IsActive;
        jobPost.UpdatedAt = DateTime.UtcNow;

        // Update required skills
        jobPost.JobPostSkills?.Clear();
        if (request.RequiredSkillIds.Any())
        {
            foreach (var skillId in request.RequiredSkillIds)
            {
                var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
                if (skill is not null)
                {
                    jobPost.JobPostSkills?.Add(new JobPostSkill { SkillId = skillId });
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

        _unitOfWork.JobPosts.Delete(jobPost);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Job posting {JobPostingId} deleted", jobPostingId);
    }

    public async Task<IEnumerable<JobPostingDto>> SearchJobPostingsAsync(string searchTerm, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllActiveJobPostingsAsync(pageNumber, pageSize);

        ValidatePagination(pageNumber, pageSize);
        var term = searchTerm.ToLower();

        var jobPosts = await _unitOfWork.JobPosts.FindAsync(
            jp => jp.IsActive && (
                jp.Title.ToLower().Contains(term) ||
                jp.Description.ToLower().Contains(term)
            )
        );

        return jobPosts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
    }

    public async Task<IEnumerable<JobPostingDto>> GetJobPostingsBySkillAsync(int skillId, int pageNumber = 1, int pageSize = 20)
    {
        ValidatePagination(pageNumber, pageSize);

        var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);
        if (skill is null)
            throw new ArgumentException("Skill not found");

        var jobPosts = await _unitOfWork.JobPosts.FindAsync(
            jp => jp.IsActive && jp.JobPostSkills.Any(jps => jps.SkillId == skillId)
        );

        return jobPosts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
    }

    public async Task<IEnumerable<JobPostingDto>> GetJobPostingsByTypeAsync(string employmentType, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(employmentType))
            throw new ArgumentException("Employment type is required");

        ValidatePagination(pageNumber, pageSize);

        // For now, filter by description since JobPost model doesn't have EmploymentType
        var jobPosts = await _unitOfWork.JobPosts.FindAsync(jp => jp.IsActive);

        return jobPosts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
    }

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
            RequiredSkills = jobPost.JobPostSkills?.Select(jps => jps.Skill.Name).ToList() ?? new(),
            ApplicationCount = jobPost.JobApplications?.Count ?? 0,
            CreatedAt = jobPost.CreatedAt,
            UpdatedAt = jobPost.UpdatedAt ?? DateTime.UtcNow
        };
    }

    private static void ValidateCreateJobPostingInput(CreateJobPostingDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 100)
            throw new ArgumentException("Title is required (max 100 characters)");

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 5000)
            throw new ArgumentException("Description is required (max 5000 characters)");

        if (request.ApplicationDeadline.HasValue && request.ApplicationDeadline.Value < DateTime.UtcNow)
            throw new ArgumentException("Application deadline must be in the future");
    }

    private static void ValidateUpdateJobPostingInput(UpdateJobPostingDto request)
    {
        ValidateCreateJobPostingInput(new CreateJobPostingDto
        {
            Title = request.Title,
            Description = request.Description,
            ApplicationDeadline = request.ApplicationDeadline,
            Requirements = "",
            Location = "",
            EmploymentType = "",
            RequiredSkillIds = request.RequiredSkillIds
        });
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("Page size must be between 1 and 100");
    }
}
