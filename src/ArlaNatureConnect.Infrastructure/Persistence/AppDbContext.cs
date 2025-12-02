using ArlaNatureConnect.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Persistence;

public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Farm> Farms { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<NatureCheckCase> NatureCheckCases { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure table names to match database schema
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Address"); // Database table is singular "Address", not "Addresses"
        });

        // Configure automatic inclusion of related entities
        modelBuilder.Entity<Person>().Navigation(e => e.Role).AutoInclude();
        modelBuilder.Entity<Person>().Navigation(e => e.Address).AutoInclude();
        modelBuilder.Entity<Person>().Navigation(e => e.Farms).AutoInclude();

        // Note: Farm.Person and Farm.Address are NOT auto-included because:
        // 1. Farms may have Guid.Empty for PersonId/AddressId (temporary state during creation)
        // 2. AutoInclude would fail when trying to load non-existent entities
        // Use explicit .Include() in queries when needed
    }
}
