using Domain.Models;
using Shared.Pagination;

namespace Domain.Contracts;

public interface IJobApplicationRepository : IRepositoryBase<JobApplication>
{
    Task<bool> ExistsAsync(string candidateId, int jobPostId, CancellationToken cancellationToken = default);
    Task<PagedResult<JobApplication>> GetByCandidateAsync(string candidateId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<PagedResult<JobApplication>> GetByJobPostAsync(int jobPostId, PaginationParams pagination, CancellationToken cancellationToken = default);
}
