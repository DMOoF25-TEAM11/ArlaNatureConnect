using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class Farm : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CVR { get; set; } = string.Empty;

    // Non-nullable per domain model: A farm must have an owner and an address.
    public Guid PersonId { get; set; } = Guid.Empty;
    public Guid AddressId { get; set; } = Guid.Empty;

    // Navigation properties
    public virtual Person Person { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
}
