using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories;

public class SkillRepository : RepositoryBase<Skill>, ISkillRepository
{
    public SkillRepository(AppDbContext context) : base(context) { }

    public async Task<Skill?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var nameList = names.ToList();
        return await _dbSet
            .Where(s => nameList.Contains(s.Name))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
