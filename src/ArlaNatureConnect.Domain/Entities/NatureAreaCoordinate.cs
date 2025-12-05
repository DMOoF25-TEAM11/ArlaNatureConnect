using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class NatureAreaCoordinate : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid NatureAreaId { get; set; } = Guid.Empty;
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public int OrderIndex { get; set; } = 0;
}
