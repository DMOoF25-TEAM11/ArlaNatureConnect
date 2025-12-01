namespace ArlaNatureConnect.Domain.Entities;

public class Person
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid AddressId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Address Address { get; set; } = null!;
    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();
}
