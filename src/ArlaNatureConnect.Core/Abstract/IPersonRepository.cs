using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract;

public interface IPersonRepository : IRepository<Person>
{
    Task<IEnumerable<Person>> GetPersonsByRoleAsync(string role, CancellationToken cancellationToken = default);
}
