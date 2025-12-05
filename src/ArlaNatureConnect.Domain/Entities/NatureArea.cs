using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class NatureArea : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid FarmId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<NatureAreaCoordinate> Coordinates { get; set; } = null!;
    public virtual ICollection<NatureAreaImage> Images { get; set; } = null!;
}
