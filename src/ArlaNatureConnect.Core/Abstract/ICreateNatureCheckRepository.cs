using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract;

public interface ICreateNatureCheckRepository
{
    Task<Guid> CreateNatureCheckAsync(CreateNatureCheck request, CancellationToken cancellationToken = default);

    Task<CreateNatureCheck?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
