using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IRoleRepository for prototype purposes.
/// </summary>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    // For prototype: return empty list or mock data
    // In production, this would use a database context
    public new async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Return empty list for now - can be extended with mock data if needed
        await Task.CompletedTask;
        return new List<Role>();
    }

    public new async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return null;
    }
}


