using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class RoleRepository(AppDbContext context) : Repository<Role>(context), IRoleRepository
{
}
