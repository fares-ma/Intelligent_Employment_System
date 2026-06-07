using Domain.Models;

namespace Domain.Contracts;

public interface IProcessedIdempotencyKeyRepository
{
    Task<bool> ExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task<ProcessedIdempotencyKey> AddAsync(ProcessedIdempotencyKey entity, CancellationToken cancellationToken = default);
}
