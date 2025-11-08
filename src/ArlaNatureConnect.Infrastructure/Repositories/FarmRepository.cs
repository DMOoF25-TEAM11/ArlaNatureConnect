using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class FarmRepository : Repository<Farm>, IFarmRepository
{
    public FarmRepository(AppDbContext context) : base(context)
    {
    }
}
