using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class FarmRepository : Repository<Farm>, IFarmRepository
{
    public FarmRepository(IDbContextFactory<AppDbContext> factory) : base(factory)
    {
    }
}
