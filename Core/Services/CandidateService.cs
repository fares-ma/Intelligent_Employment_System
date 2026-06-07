using AutoMapper;
using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidates;
using Shared;
using Shared.Pagination;

namespace Services;

public class CandidateService : ICandidateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly ILogger<CandidateService> _logger;

    public CandidateService(
        IUnitOfWork unitOfWork,
        ICandidateRepository candidateRepository,
        IResumeRepository resumeRepository,
        ISavedJobRepository savedJobRepository,
        ISkillRepository skillRepository,
        IFileStorageService fileStorageService,
        IMapper mapper,
        ILogger<CandidateService> logger)
    {
        _unitOfWork = unitOfWork;
        _candidateRepository = candidateRepository;
        _resumeRepository = resumeRepository;
        _savedJobRepository = savedJobRepository;
        _skillRepository = skillRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CandidateProfileDto> GetProfileAsync(string candidateId)
    {
        var candidate = await _candidateRepository.GetWithSkillsAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        var resumesData = await _resumeRepository.GetByCandidateAsync(candidateId);
        
        var dto = _mapper.Map<CandidateProfileDto>(candidate);
        dto.Resumes = _mapper.Map<List<ResumeDto>>(resumesData);
        
        return dto;
    }

    public async Task<CandidateProfileDto> UpdateProfileAsync(string candidateId, UpdateCandidateProfileDto dto)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        _mapper.Map(dto, candidate);
        candidate.UpdatedAt = DateTime.UtcNow;

        _candidateRepository.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        return await GetProfileAsync(candidateId);
    }

    public async Task UpdateSkillsAsync(string candidateId, UpdateSkillsDto dto)
    {
        var candidate = await _candidateRepository.GetWithSkillsAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // Clear existing skills
        candidate.CandidateSkills.Clear();

        // Normalize names
        var normalizedNames = dto.SkillNames.Select(n => n.Trim().ToLower()).Distinct().ToList();

        // Batch fetch existing skills instead of N+1 queries
        var existingSkills = (await _skillRepository.GetByNamesAsync(normalizedNames)).ToList();
        var existingNames = existingSkills.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Create missing skills in bulk
        var missingNames = normalizedNames.Where(n => !existingNames.Contains(n)).ToList();
        var newSkills = new List<Skill>();
        foreach (var name in missingNames)
        {
            var skill = new Skill { Name = name };
            _skillRepository.Create(skill);
            newSkills.Add(skill);
        }

        if (newSkills.Count > 0)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex) when (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true
                                    || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Race condition: another request created the skill. Re-query.
                _logger.LogWarning("Duplicate skill detected during bulk create, re-querying");
                existingSkills = (await _skillRepository.GetByNamesAsync(normalizedNames)).ToList();
                newSkills.Clear();
            }
        }

        // Link all skills to candidate
        var allSkills = existingSkills.Concat(newSkills);
        foreach (var skill in allSkills)
        {
            candidate.CandidateSkills.Add(new CandidateSkill { CandidateId = candidateId, SkillId = skill.Id });
        }

        _candidateRepository.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Skills updated for candidate {CandidateId}: {Count} skills", candidateId, normalizedNames.Count);
    }

    public async Task<ResumeDto> UploadResumeAsync(string candidateId, string fileName, Stream fileStream)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // Save file first
        string filePath = await _fileStorageService.SaveFileAsync(fileName, fileStream, "resumes");

        // Determine file size safely (stream may no longer be seekable after save)
        long fileSizeBytes = fileStream.CanSeek ? fileStream.Length : new FileInfo(filePath).Length;

        // Create resume entity — wrap in try/catch to clean up orphaned file on DB failure
        try
        {
            var resume = new Resume
            {
                CandidateId = candidateId,
                OriginalFileName = fileName,
                StoredFilePath = filePath,
                FileType = Path.GetExtension(fileName).TrimStart('.').ToLower(),
                FileSizeBytes = fileSizeBytes,
                CreatedAt = DateTime.UtcNow
            };

            _resumeRepository.Create(resume);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Resume uploaded for candidate {CandidateId}: {FileName}", candidateId, fileName);

            return _mapper.Map<ResumeDto>(resume);
        }
        catch (Exception ex)
        {
            // Clean up the saved file to prevent orphans
            _logger.LogError(ex, "DB save failed after file upload for candidate {CandidateId}, cleaning up file {FilePath}", candidateId, filePath);
            try { await _fileStorageService.DeleteFileAsync(filePath); }
            catch (Exception cleanupEx) { _logger.LogError(cleanupEx, "Failed to clean up orphaned file: {FilePath}", filePath); }
            throw;
        }
    }

    public async Task DeleteResumeAsync(string candidateId, int resumeId)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId);
        if (resume == null)
            throw new NotFoundException("Resume not found");

        if (resume.CandidateId != candidateId)
            throw new ForbiddenException("You cannot delete this resume");

        // Delete physical file
        if (_fileStorageService.FileExists(resume.StoredFilePath))
            await _fileStorageService.DeleteFileAsync(resume.StoredFilePath);

        _resumeRepository.Delete(resume);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Resume deleted: {ResumeId}", resumeId);
    }

    public async Task<PagedResult<CandidateApplicationDto>> GetApplicationsAsync(string candidateId, PaginationParams paginationParams)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // TODO: Implement when JobApplication service is ready
        return new PagedResult<CandidateApplicationDto>
        {
            Items = new List<CandidateApplicationDto>(),
            TotalCount = 0,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task<PagedResult<dynamic>> GetSavedJobsAsync(string candidateId, PaginationParams paginationParams)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // TODO: Implement when JobPost repository is ready
        return new PagedResult<dynamic>
        {
            Items = new List<dynamic>(),
            TotalCount = 0,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize
        };
    }

    public async Task ToggleSaveJobAsync(string candidateId, int jobPostId)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // TODO: Implement when JobPost entity is fully set up
        await Task.CompletedTask;
    }

    public async Task<string> UpdateProfilePictureAsync(string candidateId, string fileName, Stream fileStream)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // Remember old path for cleanup after successful persistence
        string? oldPicturePath = candidate.ProfilePicturePath;

        // Save new picture FIRST (don't delete old yet — prevents data loss if save fails)
        string picturePath = await _fileStorageService.SaveFileAsync(fileName, fileStream, "profile-pictures");

        try
        {
            candidate.ProfilePicturePath = picturePath;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // DB persistence failed — clean up the newly saved file
            _logger.LogError(ex, "Failed to persist profile picture for candidate {CandidateId}, cleaning up", candidateId);
            try { await _fileStorageService.DeleteFileAsync(picturePath); }
            catch (Exception cleanupEx) { _logger.LogError(cleanupEx, "Failed to clean up new profile picture: {Path}", picturePath); }
            throw;
        }

        // Only delete old picture AFTER successful persistence
        if (!string.IsNullOrEmpty(oldPicturePath) && _fileStorageService.FileExists(oldPicturePath))
        {
            try { await _fileStorageService.DeleteFileAsync(oldPicturePath); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old profile picture: {Path}", oldPicturePath); }
        }

        _logger.LogInformation("Profile picture updated for candidate {CandidateId}", candidateId);

        return picturePath;
    }
}
