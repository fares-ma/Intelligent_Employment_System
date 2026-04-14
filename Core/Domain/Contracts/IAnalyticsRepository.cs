using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IAnalyticsRepository
    {
        Task<int> GetActiveJobsCountAsync(int companyId, CancellationToken cancellationToken = default);
        Task<int> GetTotalApplicantsAsync(int companyId, CancellationToken cancellationToken = default);
        Task<int> GetHiredCountAsync(int companyId, CancellationToken cancellationToken = default);
        Task<IEnumerable<KeyValuePair<string, int>>> GetApplicationTrendsAsync(int companyId, int days, CancellationToken cancellationToken = default);

        Task<int> GetCandidateApplicationCountAsync(string candidateId, CancellationToken cancellationToken = default);
        Task<int> GetCandidateSavedJobsCountAsync(string candidateId, CancellationToken cancellationToken = default);
        Task<int> GetCandidateUpcomingInterviewsCountAsync(string candidateId, CancellationToken cancellationToken = default);
    }
}
