using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

// Purpose: SQL Server repository for Role lookups used across UC flows.
// Notes: Extends the base repository with case-sensitive name resolution for service orchestration.
public class RoleRepository(AppDbContext context) : Repository<Role>(context), IRoleRepository
{
    // Purpose: Repository for Role lookups.
    // Notes: Adds name-based lookup support required by UC002B services.
    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
            .ConfigureAwait(false);
    }
}
