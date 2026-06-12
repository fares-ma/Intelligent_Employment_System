using AutoMapper;
using Domain.Contracts;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Services.Abstractions;
using Services.Abstractions.DTOs.Assessment;
using Shared.Pagination;
using System.Text.Json;

namespace Services;

public class AssessmentService : IAssessmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AssessmentService> _logger;

    public AssessmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AssessmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentDto request, string userId)
    {
        var jobPost = await _unitOfWork.JobPosts.GetByIdAsync(request.JobPostId);
        if (jobPost == null)
            throw new NotFoundException($"Job post {request.JobPostId} not found");

        var assessment = _mapper.Map<Assessment>(request);
        assessment.TotalScore = assessment.Questions.Sum(q => q.Points);
        
        _unitOfWork.Assessments.Create(assessment);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<AssessmentDto>(assessment);
    }

    public async Task<AssessmentDto> GetAssessmentAsync(int assessmentId)
    {
        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);
        if (assessment == null)
            throw new NotFoundException($"Assessment {assessmentId} not found");

        return _mapper.Map<AssessmentDto>(assessment);
    }

    public async Task<IEnumerable<AssessmentDto>> GetAssessmentsForJobAsync(int jobPostId)
    {
        var assessments = await _unitOfWork.Assessments.GetByJobPostAsync(jobPostId);
        return _mapper.Map<IEnumerable<AssessmentDto>>(assessments);
    }

    public async Task<PagedResult<AssessmentCandidateListDto>> GetAssessmentCandidatesAsync(int assessmentId, PaginationParams pagination)
    {
        var pagedResults = await _unitOfWork.CandidateAssessments.GetPagedByAssessmentAsync(assessmentId, pagination);

        var dtoList = pagedResults.Items.Select(ca => new AssessmentCandidateListDto
        {
            CandidateId = ca.CandidateId,
            FullName = ca.Candidate?.UserName ?? "Unknown", // Assuming UserName acts as full name if no profile
            Email = ca.Candidate?.Email ?? "Unknown",
            Score = ca.Score,
            Status = ca.IsCompleted ? "Completed" : "In Progress",
            SubmissionDate = ca.SubmittedAt
        }).ToList();

        return new PagedResult<AssessmentCandidateListDto>
        {
            Items = dtoList,
            TotalCount = pagedResults.TotalCount,
            PageNumber = pagedResults.PageNumber,
            PageSize = pagedResults.PageSize
        };
    }

    public async Task<AssessmentCandidateDetailDto> GetCandidateAssessmentDetailsAsync(int assessmentId, string candidateId)
    {
        var ca = await _unitOfWork.CandidateAssessments.GetByCandidateAndAssessmentAsync(candidateId, assessmentId);
        if (ca == null)
            throw new NotFoundException("Assessment attempt not found.");

        var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);

        var dto = new AssessmentCandidateDetailDto
        {
            CandidateId = candidateId,
            FullName = candidate?.UserName ?? "Unknown",
            Email = candidate?.Email ?? "Unknown",
            PhoneNumber = candidate?.PhoneNumber,
            AssessmentId = assessmentId,
            AssessmentName = assessment?.Title ?? "Unknown",
            TotalScore = ca.Score ?? 0,
            MaximumScore = assessment?.TotalScore ?? 0,
            SubmissionDate = ca.SubmittedAt
        };

        if (assessment != null && !string.IsNullOrEmpty(ca.Answers))
        {
            try
            {
                var answersDict = JsonSerializer.Deserialize<Dictionary<string, string>>(ca.Answers) ?? new Dictionary<string, string>();
                foreach (var q in assessment.Questions.OrderBy(q => q.OrderIndex))
                {
                    string candAnswer = answersDict.TryGetValue(q.Id.ToString(), out var ans) ? ans : "";
                    
                    bool isCorrect = false;
                    if (q.Type == Domain.Enums.QuestionType.MCQ || q.Type == Domain.Enums.QuestionType.TrueFalse)
                    {
                        isCorrect = !string.IsNullOrEmpty(q.CorrectAnswer) && q.CorrectAnswer.Equals(candAnswer, StringComparison.OrdinalIgnoreCase);
                    }

                    dto.Questions.Add(new QuestionAnswerDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Text,
                        QuestionType = q.Type.ToString(),
                        CandidateAnswer = candAnswer,
                        CorrectAnswer = q.CorrectAnswer,
                        IsCorrect = isCorrect,
                        Points = q.Points
                    });
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse answers for candidate {CandidateId} on assessment {AssessmentId}", candidateId, assessmentId);
            }
        }

        return dto;
    }

    public async Task<CandidateAssessmentDto> StartAssessmentAsync(int assessmentId, int jobApplicationId, string candidateId)
    {
        if (jobApplicationId <= 0)
            throw new BadRequestException("Valid jobApplicationId is required");

        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(assessmentId);
        if (assessment == null)
            throw new NotFoundException($"Assessment {assessmentId} not found");

        var jobApplication = await _unitOfWork.JobApplications.GetByIdAsync(jobApplicationId);
        if (jobApplication == null)
            throw new NotFoundException($"Job application {jobApplicationId} not found");

        if (jobApplication.CandidateId != candidateId)
            throw new UnauthorizedAccessException("You can only start assessments for your own job applications");

        if (jobApplication.JobPostId != assessment.JobPostId)
            throw new BadRequestException("Job application does not match this assessment's job posting");

        // Check if already started
        var existing = await _unitOfWork.CandidateAssessments.GetByCandidateAndAssessmentAsync(candidateId, assessmentId);
        if (existing != null)
        {
            if (existing.IsCompleted)
                throw new BadRequestException("Assessment already completed");
                
            return _mapper.Map<CandidateAssessmentDto>(existing);
        }

        var candidateAssessment = new CandidateAssessment
        {
            CandidateId = candidateId,
            AssessmentId = assessmentId,
            JobApplicationId = jobApplicationId,
            StartedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        _unitOfWork.CandidateAssessments.Create(candidateAssessment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CandidateAssessmentDto>(candidateAssessment);
    }

    public async Task<CandidateAssessmentDto> SubmitAssessmentAsync(SubmitAssessmentDto request, string candidateId)
    {
        var attempt = await _unitOfWork.CandidateAssessments.GetByCandidateAndAssessmentAsync(candidateId, request.AssessmentId);
        
        if (attempt == null)
            throw new NotFoundException("Assessment attempt not found. Please start the assessment first.");
            
        if (attempt.IsCompleted)
            throw new BadRequestException("Assessment is already submitted.");

        var assessment = await _unitOfWork.Assessments.GetWithQuestionsAsync(request.AssessmentId);
        
        // Time limit check (with 1 minute grace period)
        if (attempt.StartedAt.HasValue && assessment != null)
        {
            var allowedEndTime = attempt.StartedAt.Value.AddMinutes(assessment.TimeLimitMinutes + 1);
            if (DateTime.UtcNow > allowedEndTime)
            {
                attempt.IsCompleted = true;
                attempt.SubmittedAt = DateTime.UtcNow;
                attempt.Score = 0; // Penalty for overtime
                attempt.Answers = request.Answers;
                
                _unitOfWork.CandidateAssessments.Update(attempt);
                await _unitOfWork.SaveChangesAsync();
                
                throw new BadRequestException("Assessment time limit exceeded.");
            }
        }

        attempt.Answers = request.Answers;
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.IsCompleted = true;
        
        // Simple auto-grade logic for demonstration (assuming JSON answers)
        // In reality, we would parse JSON answers and compare with CorrectAnswer
        attempt.Score = 0; // Needs grading logic for real deployment

        _unitOfWork.CandidateAssessments.Update(attempt);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CandidateAssessmentDto>(attempt);
    }

    public async Task<CandidateAssessmentDto> GetCandidateAssessmentAsync(int assessmentId, string candidateId)
    {
        var attempt = await _unitOfWork.CandidateAssessments.GetByCandidateAndAssessmentAsync(candidateId, assessmentId);
        if (attempt == null)
            throw new NotFoundException("Assessment attempt not found.");

        return _mapper.Map<CandidateAssessmentDto>(attempt);
    }

    public async Task<IEnumerable<CandidateAssessmentDto>> GetCandidateAssessmentsAsync(string candidateId)
    {
        var attempts = await _unitOfWork.CandidateAssessments.GetByCandidateAsync(candidateId);
        return _mapper.Map<IEnumerable<CandidateAssessmentDto>>(attempts);
    }
}
