namespace ArlaNatureConnect.Domain.Entities;

public class Farm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CVR { get; set; } = string.Empty;
    public Guid PersonId { get; set; }
    public Guid AddressId { get; set; }

    // Navigation properties
    public virtual Person Person { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
}
