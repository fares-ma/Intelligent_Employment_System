using Domain.Models;

namespace Domain.Contracts;

public interface ISkillRepository : IRepositoryBase<Skill>
{
    Task<Skill?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}
