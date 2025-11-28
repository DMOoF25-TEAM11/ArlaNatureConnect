using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract.Services;

public interface IPersonQueryService
{
    Task<IEnumerable<Person>> GetAllWithRolesAsync(CancellationToken ct = default);
}
