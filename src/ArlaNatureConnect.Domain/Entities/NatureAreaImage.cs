using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class NatureAreaImage : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid NatureAreaId { get; set; } = Guid.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
