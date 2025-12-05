using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class NatureAreaRepository(IDbContextFactory<AppDbContext> factory) : Repository<NatureArea>(factory), INatureAreaRepository
{
}
