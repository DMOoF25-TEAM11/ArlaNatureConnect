using ArlaNatureConnect.Domain.Abstract;

namespace ArlaNatureConnect.Domain.Entities;

public class Address : IEntity
{
    public Guid Id { get; set; }
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}
