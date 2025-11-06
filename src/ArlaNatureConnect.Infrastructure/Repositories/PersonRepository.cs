using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IPersonRepository for prototype purposes.
/// </summary>
public class PersonRepository : Repository<Person>, IPersonRepository
{
    // For prototype: return empty list or mock data
    // In production, this would use a database context
    public new async Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Return empty list for now - can be extended with mock data if needed
        await Task.CompletedTask;
        return new List<Person>();
    }

    public new async Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return null;
    }
}


