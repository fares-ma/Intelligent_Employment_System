using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class AssessmentRepository : RepositoryBase<Assessment>, IAssessmentRepository
{
    public AssessmentRepository(AppDbContext context) : base(context) { }

    public async Task<Assessment?> GetWithQuestionsAsync(int assessmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Questions)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assessmentId, cancellationToken);
    }

    public async Task<IEnumerable<Assessment>> GetByJobPostAsync(int jobPostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.JobPostId == jobPostId)
            .Include(a => a.Questions)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAttemptAsync(CandidateAssessment attempt, CancellationToken cancellationToken = default)
    {
        await _context.Set<CandidateAssessment>().AddAsync(attempt, cancellationToken);
    }

    public async Task<CandidateAssessment?> GetAttemptAsync(int attemptId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CandidateAssessment>()
            .Include(ca => ca.Candidate)
            .FirstOrDefaultAsync(ca => ca.Id == attemptId, cancellationToken);
    }

    public async Task<CandidateAssessment?> GetAttemptByCandidateAndAssessmentAsync(string candidateId, int assessmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CandidateAssessment>()
            .FirstOrDefaultAsync(ca => ca.CandidateId == candidateId && ca.AssessmentId == assessmentId, cancellationToken);
    }

    public void UpdateAttempt(CandidateAssessment attempt)
    {
        _context.Set<CandidateAssessment>().Update(attempt);
    }

    public async Task<IEnumerable<CandidateAssessment>> GetAttemptsByAssessmentAsync(int assessmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CandidateAssessment>()
            .Include(ca => ca.Candidate)
            .Where(ca => ca.AssessmentId == assessmentId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
