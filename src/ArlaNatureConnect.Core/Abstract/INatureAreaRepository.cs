using ArlaNatureConnect.Domain.Entities;

namespace ArlaNatureConnect.Core.Abstract;

public interface INatureAreaRepository : IRepository<NatureArea>
{
    Task<IEnumerable<NatureArea>> GetByFarmIdAsync(Farm farm);
}
