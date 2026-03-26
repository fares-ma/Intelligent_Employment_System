using AutoMapper;
using Domain.Contracts;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Assessment;
using Services.Abstractions.DTOs.JobApplication;
using Shared.Pagination;
using System.Text.Json;

namespace Services;

public class AssessmentService : IAssessmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiServiceClient _aiServiceClient;
    private readonly IMapper _mapper;
    private readonly ILogger<AssessmentService> _logger;

    public AssessmentService(IUnitOfWork unitOfWork, IAiServiceClient aiServiceClient, IMapper mapper, ILogger<AssessmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _aiServiceClient = aiServiceClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AssessmentDetailDto> CreateAssessmentAsync(string recruiterId, CreateAssessmentDto request)
    {
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(request.JobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("Not authorized to add assessments to this job.");

        var assessment = _mapper.Map<Assessment>(request);
        
        if (request.IsAiGenerated)
        {
            // Placeholder for AI Generation mapping if we dynamically generate here.
            _logger.LogInformation("AI Generated Assessment requested. Using provided questions for now.");
        }

        _unitOfWork.Assessments.Create(assessment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AssessmentDetailDto>(assessment);
    }

    public async Task<AssessmentDetailDto> UpdateAssessmentAsync(int assessmentId, string recruiterId, UpdateAssessmentDto request)
    {
        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);
        if (assessment == null) throw new ArgumentException("Assessment not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(assessment.JobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("Not authorized.");

        _mapper.Map(request, assessment);
        assessment.TotalScore = assessment.Questions.Sum(q => q.Points);

        _unitOfWork.Assessments.Update(assessment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AssessmentDetailDto>(assessment);
    }

    public async Task DeleteAssessmentAsync(int assessmentId, string recruiterId)
    {
        var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
        if (assessment == null) return;

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(assessment.JobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("Not authorized.");

        _unitOfWork.Assessments.Delete(assessment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssessmentListDto>> GetJobAssessmentsAsync(int jobPostId, string recruiterId)
    {
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(jobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("Not authorized.");

        var assessments = await _unitOfWork.Assessments.GetByJobPostAsync(jobPostId);
        return _mapper.Map<IEnumerable<AssessmentListDto>>(assessments);
    }

    public async Task<AssessmentDetailDto> GetAssessmentAsync(int assessmentId, string userId)
    {
        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);
        if (assessment == null) throw new ArgumentException("Assessment not found");

        var dto = _mapper.Map<AssessmentDetailDto>(assessment);

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(assessment.JobPostId);
        
        bool isRecruiterOwner = jobPost?.CreatedByRecruiterId == userId;
        if (!isRecruiterOwner)
        {
            // Candidate view - Hide correct answers
            foreach (var q in dto.Questions)
            {
                q.CorrectAnswer = null;
            }
        }
        
        return dto;
    }

    public async Task<CandidateAssessmentAttemptDto> StartAssessmentAsync(int assessmentId, string candidateId)
    {
        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);
        if (assessment == null || !assessment.IsActive) throw new ArgumentException("Assessment not found or inactive.");

        var applications = await _unitOfWork.JobApplications.FindAsync(a => a.CandidateId == candidateId && a.JobPostId == assessment.JobPostId);
        var application = applications.FirstOrDefault();
        if (application == null || application.Status != ApplicationStatus.Assessment)
            throw new UnauthorizedAccessException("Candidate is not at the assessment stage for this job.");

        // Check existing attempt
        var existingAttempt = await _unitOfWork.Assessments.GetAttemptByCandidateAndAssessmentAsync(candidateId, assessmentId);
        if (existingAttempt != null)
            throw new ArgumentException("Already attempted this assessment");

        var attempt = new CandidateAssessment
        {
            CandidateId = candidateId,
            AssessmentId = assessmentId,
            JobApplicationId = application.Id,
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        await _unitOfWork.Assessments.AddAttemptAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        var dto = _mapper.Map<CandidateAssessmentAttemptDto>(assessment);
        dto.CandidateAssessmentId = attempt.Id;
        dto.StartedAt = attempt.StartedAt.Value;
        dto.DeadlineAt = attempt.StartedAt.Value.AddMinutes(assessment.TimeLimitMinutes);
        
        foreach (var q in dto.Questions)
        {
            q.CorrectAnswer = null; // Hide answers
        }

        return dto;
    }

    public async Task<SubmitAssessmentResultDto> SubmitAssessmentAsync(int assessmentId, string candidateId, SubmitAssessmentDto request)
    {
        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);
        if (assessment == null) throw new ArgumentException("Assessment not found.");

        var attempt = await _unitOfWork.Assessments.GetAttemptByCandidateAndAssessmentAsync(candidateId, assessmentId);
        if (attempt == null) throw new UnauthorizedAccessException("Active attempt not found.");
        if (attempt.IsCompleted) throw new ArgumentException("Assessment already submitted.");

        // Validate time
        if (DateTime.UtcNow > attempt.StartedAt.Value.AddMinutes(assessment.TimeLimitMinutes).AddMinutes(1))
        {
            attempt.IsCompleted = true;
            attempt.SubmittedAt = DateTime.UtcNow;
            attempt.Score = 0;
            _unitOfWork.Assessments.UpdateAttempt(attempt);
            await _unitOfWork.SaveChangesAsync();
            throw new ArgumentException("Time window expired.");
        }

        decimal totalScore = 0;
        foreach (var answerDto in request.Answers)
        {
            var question = assessment.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
            if (question != null)
            {
                if ((question.Type == QuestionType.MCQ || question.Type == QuestionType.TrueFalse) 
                    && string.Equals(question.CorrectAnswer, answerDto.Answer, StringComparison.OrdinalIgnoreCase))
                {
                    totalScore += question.Points;
                }
            }
        }

        attempt.Answers = JsonSerializer.Serialize(request.Answers);
        attempt.Score = totalScore;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.IsCompleted = true;

        _unitOfWork.Assessments.UpdateAttempt(attempt);
        await _unitOfWork.SaveChangesAsync();

        return new SubmitAssessmentResultDto
        {
            CandidateAssessmentId = attempt.Id,
            Score = attempt.Score,
            TotalScore = assessment.TotalScore,
            IsCompleted = attempt.IsCompleted
        };
    }

    public async Task<PagedResult<CandidateAssessmentResultDto>> GetAssessmentResultsAsync(int assessmentId, string recruiterId, int pageNumber = 1, int pageSize = 20)
    {
        var assessment = await _unitOfWork.Assessments.GetByIdAsync(assessmentId);
        if (assessment == null) throw new ArgumentException("Assessment not found");

        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(assessment.JobPostId);
        if (jobPost == null || jobPost.CreatedByRecruiterId != recruiterId)
            throw new UnauthorizedAccessException("Not authorized.");

        var allAttempts = await _unitOfWork.Assessments.GetAttemptsByAssessmentAsync(assessmentId);

        var count = allAttempts.Count();
        var pageItems = allAttempts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var dtos = pageItems.Select(a => new CandidateAssessmentResultDto
        {
            CandidateAssessmentId = a.Id,
            Candidate = new ApplicantCandidateDto
            {
                Id = a.Candidate.Id,
                FirstName = a.Candidate.FirstName,
                LastName = a.Candidate.LastName,
                Email = a.Candidate.Email ?? string.Empty,
                JobTitle = a.Candidate.JobTitle,
                YearsOfExperience = a.Candidate.YearsOfExperience
            },
            Score = a.Score,
            TotalScore = assessment.TotalScore,
            StartedAt = a.StartedAt,
            SubmittedAt = a.SubmittedAt,
            IsCompleted = a.IsCompleted
        }).ToList();

        return new PagedResult<CandidateAssessmentResultDto>(dtos, count, pageNumber, pageSize);
    }
}
