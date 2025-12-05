using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class NatureAreaImageRepository(IDbContextFactory<AppDbContext> factory) : Repository<NatureAreaImage>(factory), INatureAreaImageRepository
{
}
