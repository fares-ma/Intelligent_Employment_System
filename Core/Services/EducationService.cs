using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidate;

namespace Services;

/// <summary>
/// Service for managing candidate education records (Add, Read, Update, Delete)
/// </summary>
public class EducationService : IEducationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EducationService> _logger;

    public EducationService(IUnitOfWork unitOfWork, ILogger<EducationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CandidateEducationDto> AddEducationAsync(string candidateId, CreateEducationDto request)
    {
        _logger.LogInformation("Adding education for candidate {CandidateId}", candidateId);

        // Validate input
        ValidateEducationInput(request);

        // Verify candidate exists
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
        {
            _logger.LogWarning("Candidate not found: {CandidateId}", candidateId);
            throw new NotFoundException($"Candidate with ID '{candidateId}' not found");
        }

        // Create education record
        var education = new CandidateEducation
        {
            CandidateId = candidateId,
            Degree = request.Degree,
            FieldOfStudy = request.FieldOfStudy,
            Institution = request.Institution,
            GraduationYear = request.GraduationYear,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.CandidateEducations.Create(education);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Education added successfully for candidate {CandidateId}", candidateId);

        return MapToDto(education);
    }

    public async Task<IEnumerable<CandidateEducationDto>> GetCandidateEducationsAsync(string candidateId)
    {
        _logger.LogInformation("Fetching education records for candidate {CandidateId}", candidateId);

        // Verify candidate exists
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
        {
            throw new NotFoundException($"Candidate with ID '{candidateId}' not found");
        }

        var educations = await _unitOfWork.CandidateEducations.GetByCandidateAsync(candidateId);
        return educations.Select(MapToDto).ToList();
    }

    public async Task<CandidateEducationDto> GetEducationAsync(string candidateId, string educationId)
    {
        _logger.LogInformation("Fetching education {EducationId} for candidate {CandidateId}", educationId, candidateId);

        if (!int.TryParse(educationId, out int id))
            throw new BadRequestException("Invalid ID format");
            
        var education = await _unitOfWork.CandidateEducations.GetByIdAsync(id);
        if (education is null)
        {
            throw new NotFoundException($"Education record with ID '{educationId}' not found");
        }

        if (education.CandidateId != candidateId)
        {
            _logger.LogWarning("Candidate {CandidateId} attempted to access education {EducationId} they don't own", candidateId, educationId);
            throw new ForbiddenException("You don't have permission to access this education record");
        }

        return MapToDto(education);
    }

    public async Task<CandidateEducationDto> UpdateEducationAsync(string candidateId, string educationId, UpdateEducationDto request)
    {
        _logger.LogInformation("Updating education {EducationId} for candidate {CandidateId}", educationId, candidateId);

        // Validate input
        ValidateUpdateEducationInput(request);

        // Get education record
        if (!int.TryParse(educationId, out int id))
            throw new BadRequestException("Invalid ID format");
            
        var education = await _unitOfWork.CandidateEducations.GetByIdAsync(id);
        if (education is null)
        {
            throw new NotFoundException($"Education record with ID '{educationId}' not found");
        }

        // Verify ownership
        if (education.CandidateId != candidateId)
        {
            _logger.LogWarning("Candidate {CandidateId} attempted to update education {EducationId} they don't own", candidateId, educationId);
            throw new ForbiddenException("You don't have permission to update this education record");
        }

        // Update fields
        education.Degree = request.Degree;
        education.FieldOfStudy = request.FieldOfStudy;
        education.Institution = request.Institution;
        education.GraduationYear = request.GraduationYear;

        _unitOfWork.CandidateEducations.Update(education);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Education {EducationId} updated successfully", educationId);

        return MapToDto(education);
    }

    public async Task DeleteEducationAsync(string candidateId, string educationId)
    {
        _logger.LogInformation("Deleting education {EducationId} for candidate {CandidateId}", educationId, candidateId);

        // Get education record
        if (!int.TryParse(educationId, out int id))
            throw new BadRequestException("Invalid ID format");
            
        var education = await _unitOfWork.CandidateEducations.GetByIdAsync(id);
        if (education is null)
        {
            throw new NotFoundException($"Education record with ID '{educationId}' not found");
        }

        // Verify ownership
        if (education.CandidateId != candidateId)
        {
            _logger.LogWarning("Candidate {CandidateId} attempted to delete education {EducationId} they don't own", candidateId, educationId);
            throw new ForbiddenException("You don't have permission to delete this education record");
        }

        _unitOfWork.CandidateEducations.Delete(education);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Education {EducationId} deleted successfully", educationId);
    }

    private static void ValidateEducationInput(CreateEducationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Degree) || request.Degree.Length < 1 || request.Degree.Length > 50)
            throw new BadRequestException("Degree must be between 1 and 50 characters");

        if (string.IsNullOrWhiteSpace(request.FieldOfStudy) || request.FieldOfStudy.Length < 1 || request.FieldOfStudy.Length > 100)
            throw new BadRequestException("Field of Study must be between 1 and 100 characters");

        if (string.IsNullOrWhiteSpace(request.Institution) || request.Institution.Length < 1 || request.Institution.Length > 100)
            throw new BadRequestException("Institution must be between 1 and 100 characters");

        if (request.GraduationYear < 1900 || request.GraduationYear > 2100)
            throw new BadRequestException("Graduation year must be between 1900 and 2100");
    }

    private static void ValidateUpdateEducationInput(UpdateEducationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Degree) || request.Degree.Length < 1 || request.Degree.Length > 50)
            throw new BadRequestException("Degree must be between 1 and 50 characters");

        if (string.IsNullOrWhiteSpace(request.FieldOfStudy) || request.FieldOfStudy.Length < 1 || request.FieldOfStudy.Length > 100)
            throw new BadRequestException("Field of Study must be between 1 and 100 characters");

        if (string.IsNullOrWhiteSpace(request.Institution) || request.Institution.Length < 1 || request.Institution.Length > 100)
            throw new BadRequestException("Institution must be between 1 and 100 characters");

        if (request.GraduationYear < 1900 || request.GraduationYear > 2100)
            throw new BadRequestException("Graduation year must be between 1900 and 2100");
    }

    private static CandidateEducationDto MapToDto(CandidateEducation education)
    {
        return new CandidateEducationDto
        {
            Id = education.Id.ToString(),
            Degree = education.Degree,
            FieldOfStudy = education.FieldOfStudy,
            Institution = education.Institution,
            GraduationYear = education.GraduationYear,
            CreatedAt = education.CreatedAt
        };
    }
}
