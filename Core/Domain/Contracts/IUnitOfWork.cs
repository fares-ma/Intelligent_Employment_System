namespace Domain.Contracts;

/// <summary>
/// Unit of Work — owns all repositories and coordinates SaveChanges.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ICandidateRepository Candidates { get; }
    IRecruiterRepository Recruiters { get; }
    ICompanyRepository Companies { get; }
    IJobPostRepository JobPosts { get; }
    IResumeRepository Resumes { get; }
    IJobApplicationRepository JobApplications { get; }
    ISkillRepository Skills { get; }
    IAssessmentRepository Assessments { get; }
    IInterviewRepository Interviews { get; }
    ISavedJobRepository SavedJobs { get; }
    INotificationRepository Notifications { get; }
    ICompanyInviteCodeRepository CompanyInviteCodes { get; }
    ICandidateEducationRepository CandidateEducations { get; }
    ICandidateExperienceRepository CandidateExperiences { get; }
    ICandidateAssessmentRepository CandidateAssessments { get; }
    IMessageRepository Messages { get; }
    IActivityLogRepository ActivityLogs { get; }
    IAnalyticsRepository Analytics { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
