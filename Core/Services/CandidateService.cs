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
    private readonly IAiServiceClient _aiServiceClient;
    private readonly IMapper _mapper;
    private readonly ILogger<CandidateService> _logger;

    public CandidateService(
        IUnitOfWork unitOfWork,
        ICandidateRepository candidateRepository,
        IResumeRepository resumeRepository,
        ISavedJobRepository savedJobRepository,
        ISkillRepository skillRepository,
        IFileStorageService fileStorageService,
        IAiServiceClient aiServiceClient,
        IMapper mapper,
        ILogger<CandidateService> logger)
    {
        _unitOfWork = unitOfWork;
        _candidateRepository = candidateRepository;
        _resumeRepository = resumeRepository;
        _savedJobRepository = savedJobRepository;
        _skillRepository = skillRepository;
        _fileStorageService = fileStorageService;
        _aiServiceClient = aiServiceClient;
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

        // Add new skills
        foreach (var skillName in dto.SkillNames)
        {
            var skill = await _skillRepository.GetByNameAsync(skillName);
            if (skill == null)
            {
                skill = new Skill { Name = skillName.ToLower() };
                _skillRepository.Create(skill);
                await _unitOfWork.SaveChangesAsync();
            }

            candidate.CandidateSkills.Add(new CandidateSkill { CandidateId = candidateId, SkillId = skill.Id });
        }

        _candidateRepository.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Skills updated for candidate {CandidateId}", candidateId);
    }

    public async Task<ResumeDto> UploadResumeAsync(string candidateId, string fileName, Stream fileStream)
    {
        var candidate = await _candidateRepository.GetByIdAsync(candidateId);
        if (candidate == null)
            throw new NotFoundException("Candidate not found");

        // Save file
        string filePath = await _fileStorageService.SaveFileAsync(fileName, fileStream, "resumes");

        // Create resume entity
        var resume = new Resume
        {
            CandidateId = candidateId,
            OriginalFileName = fileName,
            StoredFilePath = filePath,
            FileType = Path.GetExtension(fileName).TrimStart('.').ToLower(),
            FileSizeBytes = fileStream.Length,
            CreatedAt = DateTime.UtcNow
        };

        _resumeRepository.Create(resume);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Resume uploaded for candidate {CandidateId}: {FileName}", candidateId, fileName);

        return _mapper.Map<ResumeDto>(resume);
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

    public async Task<string> GenerateCvAsync(string candidateId, int resumeId)
    {
        var resume = await _resumeRepository.GetByIdAsync(resumeId);
        if (resume == null)
            throw new NotFoundException("Resume not found");

        if (resume.CandidateId != candidateId)
            throw new ForbiddenException("You cannot generate CV for this resume");

        try
        {
            // Get resume content (for now, using filename as placeholder)
            var resumeText = $"Resume: {resume.OriginalFileName}";
            var candidate = await _candidateRepository.GetByIdAsync(candidateId);

            var cvPath = await _aiServiceClient.GenerateCvAsync(resumeText, $"{candidate?.FirstName} {candidate?.LastName}");

            resume.AiGeneratedCvPath = cvPath;
            _resumeRepository.Update(resume);
            await _unitOfWork.SaveChangesAsync();

            return cvPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV for resume {ResumeId}", resumeId);
            throw;
        }
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

        // Delete old profile picture if exists
        if (!string.IsNullOrEmpty(candidate.ProfilePicturePath) && _fileStorageService.FileExists(candidate.ProfilePicturePath))
            await _fileStorageService.DeleteFileAsync(candidate.ProfilePicturePath);

        // Save new picture
        string picturePath = await _fileStorageService.SaveFileAsync(fileName, fileStream, "profile-pictures");

        candidate.ProfilePicturePath = picturePath;
        candidate.UpdatedAt = DateTime.UtcNow;

        _candidateRepository.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Profile picture updated for candidate {CandidateId}", candidateId);

        return picturePath;
    }
}
