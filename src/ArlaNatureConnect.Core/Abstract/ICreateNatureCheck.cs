using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract;

public interface ICreateNatureCheck
{
    Task<List<Farm>> GetFarmsAsync();
    Task<List<Person>> GetPersonsAsync();
    Task<List<NatureCheckCase>> GetNatureChecksAsync();
    Task<Guid> CreateNatureCheckAsync(CreateNatureCheck request, CancellationToken cancellationToken = default);
}
