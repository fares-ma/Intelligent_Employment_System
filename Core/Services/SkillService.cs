using Domain.Contracts;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Candidate;

namespace Services;

/// <summary>
/// Service for managing candidate skills and proficiency levels
/// </summary>
public class SkillService : ISkillService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SkillService> _logger;

    public SkillService(IUnitOfWork unitOfWork, ILogger<SkillService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CandidateSkillDto> AddSkillAsync(string candidateId, CreateSkillDto request)
    {
        _logger.LogInformation("Adding skill {SkillName} for candidate {CandidateId}", request.SkillName, candidateId);

        if (string.IsNullOrWhiteSpace(request.SkillName) || request.SkillName.Length > 100)
            throw new BadRequestException("Skill name is required (max 100 characters)");

        if ((int)request.Level < 1 || (int)request.Level > 3)
            throw new BadRequestException("Level must be between 1 (Beginner) and 3 (Expert)");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate with ID '{candidateId}' not found");

        // Get or create Skill
        var skills = await _unitOfWork.Skills.FindAsync(s => s.Name == request.SkillName);
        var skill = skills.FirstOrDefault();
        
        if (skill is null)
        {
            skill = new Skill { Name = request.SkillName };
            _unitOfWork.Skills.Create(skill);
            await _unitOfWork.SaveChangesAsync();
            // Refresh to get the ID
            skills = await _unitOfWork.Skills.FindAsync(s => s.Name == request.SkillName);
            skill = skills.First();
        }

        // Check if candidate already has this skill
        if (candidate.CandidateSkills?.Any(cs => cs.SkillId == skill.Id) == true)
            throw new BadRequestException($"Skill '{request.SkillName}' already exists in your profile");

        // Create CandidateSkill junction record
        var candidateSkill = new CandidateSkill
        {
            CandidateId = candidateId,
            SkillId = skill.Id,
            Level = request.Level
        };

        // This is tricky - we need to add to the candidate's CandidateSkills collection
        candidate.CandidateSkills ??= new List<CandidateSkill>();
        candidate.CandidateSkills.Add(candidateSkill);
        
        _unitOfWork.Candidates.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Skill added for candidate {CandidateId}", candidateId);
        return MapToDto(candidateSkill);
    }

    public async Task<IEnumerable<CandidateSkillDto>> GetCandidateSkillsAsync(string candidateId)
    {
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate with ID '{candidateId}' not found");

        var skills = candidate.CandidateSkills ?? new List<CandidateSkill>();
        return skills.Select(MapToDto).OrderBy(s => s.SkillName).ToList();
    }

    public async Task<CandidateSkillDto> GetSkillAsync(string candidateId, string skillId)
    {
        if (!int.TryParse(skillId, out var id))
            throw new BadRequestException("Invalid skill ID");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate not found");

        var candidateSkill = candidate.CandidateSkills?.FirstOrDefault(cs => cs.SkillId == id);
        if (candidateSkill is null)
            throw new NotFoundException($"Skill not found");

        return MapToDto(candidateSkill);
    }

    public async Task<CandidateSkillDto> UpdateSkillLevelAsync(string candidateId, string skillId, UpdateSkillDto request)
    {
        _logger.LogInformation("Updating skill level for {SkillId}", skillId);

        if (!int.TryParse(skillId, out var id))
            throw new BadRequestException("Invalid skill ID");

        if ((int)request.Level < 1 || (int)request.Level > 3)
            throw new BadRequestException("Level must be between 1 (Beginner) and 3 (Expert)");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate not found");

        var candidateSkill = candidate.CandidateSkills?.FirstOrDefault(cs => cs.SkillId == id);
        if (candidateSkill is null)
            throw new NotFoundException($"Skill not found");

        candidateSkill.Level = request.Level;
        _unitOfWork.Candidates.Update(candidate);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(candidateSkill);
    }

    public async Task DeleteSkillAsync(string candidateId, string skillId)
    {
        if (!int.TryParse(skillId, out var id))
            throw new BadRequestException("Invalid skill ID");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate not found");

        var candidateSkill = candidate.CandidateSkills?.FirstOrDefault(cs => cs.SkillId == id);
        if (candidateSkill is null)
            throw new NotFoundException($"Skill not found");

        candidate.CandidateSkills!.Remove(candidateSkill);
        _unitOfWork.Candidates.Update(candidate);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CandidateSkillDto>> GetSkillsByLevelAsync(string candidateId, int level)
    {
        if (level < 1 || level > 3)
            throw new BadRequestException("Level must be between 1 (Beginner) and 3 (Expert)");

        var skillLevel = (SkillLevel)level;
        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        if (candidate is null)
            throw new NotFoundException($"Candidate not found");

        var skills = candidate.CandidateSkills?.Where(cs => cs.Level == skillLevel) ?? new List<CandidateSkill>();
        return skills.Select(MapToDto).OrderBy(s => s.SkillName).ToList();
    }

    private static CandidateSkillDto MapToDto(CandidateSkill candidateSkill)
    {
        return new CandidateSkillDto
        {
            Id = candidateSkill.SkillId.ToString(),
            SkillName = candidateSkill.Skill.Name,
            Level = candidateSkill.Level,
            CreatedAt = DateTime.UtcNow
        };
    }
}
