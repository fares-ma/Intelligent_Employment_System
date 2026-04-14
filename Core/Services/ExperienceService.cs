using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidate;

namespace Services;

/// <summary>
/// Service for managing candidate work experience records (CRUD)
/// </summary>
public class ExperienceService : IExperienceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExperienceService> _logger;

    public ExperienceService(IUnitOfWork unitOfWork, ILogger<ExperienceService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CandidateExperienceDto> AddExperienceAsync(string candidateId, CreateExperienceDto request)
    {
        _logger.LogInformation("Adding experience for candidate {CandidateId}", candidateId);
        
        ValidateCreateExperienceInput(request);

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate with ID '{candidateId}' not found");

        var experience = new CandidateExperience
        {
            CandidateId = candidateId,
            JobTitle = request.JobTitle,
            Company = request.Company,
            Description = request.Description ?? string.Empty,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.CandidateExperiences.Create(experience);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Experience added for candidate {CandidateId}", candidateId);
        return MapToDto(experience);
    }

    public async Task<IEnumerable<CandidateExperienceDto>> GetCandidateExperiencesAsync(string candidateId)
    {
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate with ID '{candidateId}' not found");

        var experiences = await _unitOfWork.CandidateExperiences.GetByCandidateAsync(candidateId);
        return experiences.OrderByDescending(e => e.StartDate).Select(MapToDto).ToList();
    }

    public async Task<CandidateExperienceDto> GetExperienceAsync(string candidateId, string experienceId)
    {
        if (!int.TryParse(experienceId, out int id))
            throw new BadRequestException("Invalid ID format");
            
        var experience = await _unitOfWork.CandidateExperiences.GetByIdAsync(id);
        if (experience is null)
            throw new NotFoundException($"Experience record not found");

        if (experience.CandidateId != candidateId)
            throw new ForbiddenException("You don't have permission to access this experience record");

        return MapToDto(experience);
    }

    public async Task<CandidateExperienceDto> UpdateExperienceAsync(string candidateId, string experienceId, UpdateExperienceDto request)
    {
        _logger.LogInformation("Updating experience {ExperienceId}", experienceId);
        
        ValidateUpdateExperienceInput(request);

        if (!int.TryParse(experienceId, out int id))
            throw new BadRequestException("Invalid ID format");
            
        var experience = await _unitOfWork.CandidateExperiences.GetByIdAsync(id);
        if (experience is null)
            throw new NotFoundException($"Experience record not found");

        if (experience.CandidateId != candidateId)
            throw new ForbiddenException("You don't have permission to update this experience record");

        experience.JobTitle = request.JobTitle;
        experience.Company = request.Company;
        experience.Description = request.Description ?? string.Empty;
        experience.StartDate = request.StartDate;
        experience.EndDate = request.EndDate;

        _unitOfWork.CandidateExperiences.Update(experience);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(experience);
    }

    public async Task DeleteExperienceAsync(string candidateId, string experienceId)
    {
        if (!int.TryParse(experienceId, out int id))
            throw new BadRequestException("Invalid ID format");
            
        var experience = await _unitOfWork.CandidateExperiences.GetByIdAsync(id);
        if (experience is null)
            throw new NotFoundException($"Experience record not found");

        if (experience.CandidateId != candidateId)
            throw new ForbiddenException("You don't have permission to delete this experience record");

        _unitOfWork.CandidateExperiences.Delete(experience);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateCreateExperienceInput(CreateExperienceDto request)
    {
        if (string.IsNullOrWhiteSpace(request.JobTitle) || request.JobTitle.Length > 100)
            throw new BadRequestException("Job title is required (max 100 characters)");

        if (string.IsNullOrWhiteSpace(request.Company) || request.Company.Length > 100)
            throw new BadRequestException("Company is required (max 100 characters)");

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 1000)
            throw new BadRequestException("Description cannot exceed 1000 characters");

        if (request.StartDate == default)
            throw new BadRequestException("Start date is required");

        if (request.EndDate.HasValue && request.EndDate <= request.StartDate)
            throw new BadRequestException("End date must be after start date");
    }

    private static void ValidateUpdateExperienceInput(UpdateExperienceDto request)
    {
        if (string.IsNullOrWhiteSpace(request.JobTitle) || request.JobTitle.Length > 100)
            throw new BadRequestException("Job title is required (max 100 characters)");

        if (string.IsNullOrWhiteSpace(request.Company) || request.Company.Length > 100)
            throw new BadRequestException("Company is required (max 100 characters)");

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 1000)
            throw new BadRequestException("Description cannot exceed 1000 characters");

        if (request.StartDate == default)
            throw new BadRequestException("Start date is required");

        if (request.EndDate.HasValue && request.EndDate <= request.StartDate)
            throw new BadRequestException("End date must be after start date");
    }

    private static CandidateExperienceDto MapToDto(CandidateExperience experience)
    {
        return new CandidateExperienceDto
        {
            Id = experience.Id.ToString(),
            JobTitle = experience.JobTitle,
            Company = experience.Company,
            Description = experience.Description,
            StartDate = experience.StartDate,
            EndDate = experience.EndDate,
            CreatedAt = experience.CreatedAt
        };
    }
}
