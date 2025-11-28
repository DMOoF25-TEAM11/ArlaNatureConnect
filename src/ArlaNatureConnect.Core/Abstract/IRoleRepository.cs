namespace ArlaNatureConnect.Core.Abstract;

public interface IRoleRepository : IRepository<ArlaNatureConnect.Domain.Entities.Role>
{
    Task<ArlaNatureConnect.Domain.Entities.Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
