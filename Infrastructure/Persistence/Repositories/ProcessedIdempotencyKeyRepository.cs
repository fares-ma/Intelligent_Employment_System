using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class ProcessedIdempotencyKeyRepository : IProcessedIdempotencyKeyRepository
{
    private readonly AppDbContext _context;

    public ProcessedIdempotencyKeyRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return _context.ProcessedIdempotencyKeys
            .AsNoTracking()
            .AnyAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<ProcessedIdempotencyKey> AddAsync(
        ProcessedIdempotencyKey entity,
        CancellationToken cancellationToken = default)
    {
        await _context.ProcessedIdempotencyKeys.AddAsync(entity, cancellationToken);
        return entity;
    }
}
