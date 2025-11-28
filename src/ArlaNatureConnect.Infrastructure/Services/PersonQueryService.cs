using ArlaNatureConnect.Core.Abstract.Services;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Services;

public class PersonQueryService : IPersonQueryService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public PersonQueryService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<Person>> GetAllWithRolesAsync(CancellationToken ct = default)
    {
        await using AppDbContext db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Persons.Include(p => p.Role).AsNoTracking().ToListAsync(ct);
    }
}
