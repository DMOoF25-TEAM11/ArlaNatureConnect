using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class Role : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
