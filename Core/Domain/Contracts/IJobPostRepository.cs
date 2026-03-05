using Domain.Models;
using Shared.Pagination;

namespace Domain.Contracts;

public interface IJobPostRepository : IRepositoryBase<JobPost>
{
    Task<JobPost?> GetByIdWithSkillsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<JobPost>> GetPublishedJobsAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobPost>> GetSimilarJobsAsync(int jobPostId, int count = 5, CancellationToken cancellationToken = default);
    Task<PagedResult<JobPost>> GetCompanyJobsAsync(int companyId, PaginationParams pagination, CancellationToken cancellationToken = default);
}
