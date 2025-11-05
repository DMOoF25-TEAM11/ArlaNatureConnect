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
}
