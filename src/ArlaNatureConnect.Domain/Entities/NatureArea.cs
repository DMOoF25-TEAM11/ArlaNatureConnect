using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class NatureArea : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
}
