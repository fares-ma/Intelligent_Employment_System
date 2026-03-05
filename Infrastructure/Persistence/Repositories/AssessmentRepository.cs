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
}
