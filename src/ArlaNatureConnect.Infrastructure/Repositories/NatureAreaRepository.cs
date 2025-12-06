using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class NatureAreaRepository(IDbContextFactory<AppDbContext> factory) : Repository<NatureArea>(factory), INatureAreaRepository
{
    public async Task<IEnumerable<NatureArea>> GetByFarmIdAsync(Farm farm)
    {
        await using AppDbContext context = await _factory.CreateDbContextAsync();
        return await context.NatureAreas
            .Where(na => na.FarmId == farm.Id)
            .ToListAsync();
    }
}
