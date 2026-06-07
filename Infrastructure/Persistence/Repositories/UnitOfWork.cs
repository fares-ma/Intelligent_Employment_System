using Domain.Contracts;
using Persistence.Data;

namespace Persistence.Repositories;

/// <summary>
/// UnitOfWork — owns all repositories and wraps SaveChangesAsync.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private ICandidateRepository? _candidates;
    private IRecruiterRepository? _recruiters;
    private ICompanyRepository? _companies;
    private IJobPostRepository? _jobPosts;
    private IResumeRepository? _resumes;
    private IJobApplicationRepository? _jobApplications;
    private ISkillRepository? _skills;
    private IAssessmentRepository? _assessments;
    private IInterviewRepository? _interviews;
    private ISavedJobRepository? _savedJobs;
    private INotificationRepository? _notifications;
    private ICompanyInviteCodeRepository? _companyInviteCodes;
    private ICandidateEducationRepository? _candidateEducations;
    private ICandidateExperienceRepository? _candidateExperiences;
    private ICandidateAssessmentRepository? _candidateAssessments;
    private IMessageRepository? _messages;
    private IActivityLogRepository? _activityLogs;
    private IAnalyticsRepository? _analytics;
    private IProcessedIdempotencyKeyRepository? _processedIdempotencyKeys;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public ICandidateRepository Candidates =>
        _candidates ??= new CandidateRepository(_context);

    public IRecruiterRepository Recruiters =>
        _recruiters ??= new RecruiterRepository(_context);

    public ICompanyRepository Companies =>
        _companies ??= new CompanyRepository(_context);

    public IJobPostRepository JobPosts =>
        _jobPosts ??= new JobPostRepository(_context);

    public IResumeRepository Resumes =>
        _resumes ??= new ResumeRepository(_context);

    public IJobApplicationRepository JobApplications =>
        _jobApplications ??= new JobApplicationRepository(_context);

    public ISkillRepository Skills =>
        _skills ??= new SkillRepository(_context);

    public IAssessmentRepository Assessments =>
        _assessments ??= new AssessmentRepository(_context);

    public IInterviewRepository Interviews =>
        _interviews ??= new InterviewRepository(_context);

    public ISavedJobRepository SavedJobs =>
        _savedJobs ??= new SavedJobRepository(_context);

    public INotificationRepository Notifications =>
        _notifications ??= new NotificationRepository(_context);

    public ICompanyInviteCodeRepository CompanyInviteCodes =>
        _companyInviteCodes ??= new CompanyInviteCodeRepository(_context);

    public ICandidateEducationRepository CandidateEducations =>
        _candidateEducations ??= new CandidateEducationRepository(_context);

    public ICandidateExperienceRepository CandidateExperiences =>
        _candidateExperiences ??= new CandidateExperienceRepository(_context);

    public ICandidateAssessmentRepository CandidateAssessments =>
        _candidateAssessments ??= new CandidateAssessmentRepository(_context);

    public IMessageRepository Messages =>
        _messages ??= new MessageRepository(_context);

    public IActivityLogRepository ActivityLogs =>
        _activityLogs ??= new ActivityLogRepository(_context);

    public IAnalyticsRepository Analytics =>
        _analytics ??= new AnalyticsRepository(_context);

    public IProcessedIdempotencyKeyRepository ProcessedIdempotencyKeys =>
        _processedIdempotencyKeys ??= new ProcessedIdempotencyKeyRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
