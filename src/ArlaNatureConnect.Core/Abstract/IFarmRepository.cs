using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract;

public interface IFarmRepository : IRepository<Farm>
{
    Task<Farm?> GetByCvrAsync(string cvr, CancellationToken cancellationToken = default);
}
