using Domain.Contracts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AppDbContext _context;

        public AnalyticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetActiveJobsCountAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.JobPosts
                .CountAsync(j => j.CompanyId == companyId && j.ExpiryDate > DateTime.UtcNow, cancellationToken);
        }

        public async Task<int> GetTotalApplicantsAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobPost)
                .Where(ja => ja.JobPost.CompanyId == companyId)
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetHiredCountAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Include(ja => ja.JobPost)
                .Where(ja => ja.JobPost.CompanyId == companyId && ja.Status == ApplicationStatus.Accepted)
                .CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetApplicationTrendsAsync(int companyId, int days, CancellationToken cancellationToken = default)
        {
            var dateThreshold = DateTime.UtcNow.AddDays(-days);
            var query = await _context.JobApplications
                .Include(ja => ja.JobPost)
                .Where(ja => ja.JobPost.CompanyId == companyId && ja.AppliedAt >= dateThreshold)
                .GroupBy(ja => ja.AppliedAt.Date)
                .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                .ToListAsync(cancellationToken);

            return query.Select(q => new KeyValuePair<string, int>(q.Date, q.Count));
        }

        public async Task<int> GetCandidateApplicationCountAsync(string candidateId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications.CountAsync(ja => ja.CandidateId == candidateId, cancellationToken);
        }

        public async Task<int> GetCandidateSavedJobsCountAsync(string candidateId, CancellationToken cancellationToken = default)
        {
            return await _context.SavedJobs.CountAsync(sj => sj.CandidateId == candidateId, cancellationToken);
        }

        public async Task<int> GetCandidateUpcomingInterviewsCountAsync(string candidateId, CancellationToken cancellationToken = default)
        {
            return await _context.Interviews
                .Include(i => i.JobApplication)
                .CountAsync(i => i.JobApplication.CandidateId == candidateId && i.ScheduledAt > DateTime.UtcNow, cancellationToken);
        }
    }
}
