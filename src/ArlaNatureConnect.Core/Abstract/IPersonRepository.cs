namespace ArlaNatureConnect.Core.Abstract;

public interface IPersonRepository : IRepository<ArlaNatureConnect.Domain.Entities.Person>
{
    Task<IEnumerable<ArlaNatureConnect.Domain.Entities.Person>> GetPersonsByRoleAsync(string role, CancellationToken cancellationToken = default);
}
