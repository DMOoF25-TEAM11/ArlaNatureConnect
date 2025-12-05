using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class Farm : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CVR { get; set; } = string.Empty;

    // Non-nullable per domain model: A farm must have an owner and an address.
    [Obsolete("Use OwnerId instead", true)]
    public Guid PersonId
    {
        get { return OwnerId; }
        set { OwnerId = value; }
    }
    public Guid OwnerId { get; set; } = Guid.Empty;
    public Guid AddressId { get; set; } = Guid.Empty;

    // Navigation properties
    [Obsolete("Use Owner instead")]
    public virtual Person Person
    {
        get
        {
            return Owner;
        }
        set
        {
            Owner = value;
        }
    }
    public virtual Person Owner { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
}
