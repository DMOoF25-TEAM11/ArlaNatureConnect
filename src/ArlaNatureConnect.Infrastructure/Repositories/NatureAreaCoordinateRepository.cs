using ArlaNatureConnect.Core.Abstract;
using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Repositories;

public class NatureAreaCoordinateRepository(IDbContextFactory<AppDbContext> factory) : Repository<NatureAreaCoordinate>(factory), ICoordinatesRepository
{
}
