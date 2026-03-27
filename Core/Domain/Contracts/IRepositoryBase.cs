using System.Linq.Expressions;

namespace Domain.Contracts;

/// <summary>
/// Generic repository interface for standard CRUD operations.
/// Never returns IQueryable — only materialized results.
/// </summary>
public interface IRepositoryBase<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}
