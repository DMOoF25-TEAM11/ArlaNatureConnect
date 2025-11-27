namespace ArlaNatureConnect.Domain.Entities;

public class Farm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CVR { get; set; } = string.Empty;
    public Guid? PersonId { get; set; }  // Nullable to match database schema
    public Guid? AddressId { get; set; }  // Nullable to match database schema
}
