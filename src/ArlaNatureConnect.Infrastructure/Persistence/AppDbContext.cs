using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Persistence;

public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Farm> Farms { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<NatureCheckCase> NatureCheckCases { get; set; } = null!;
    public DbSet<NatureArea> NatureAreas { get; set; } = null!;
    public DbSet<NatureAreaCoordinate> NatureAreaCoordinates { get; set; } = null!;
    public DbSet<NatureAreaImage> NatureAreaImages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure automatic inclusion of related entities
        modelBuilder.Entity<Person>().Navigation(e => e.Role).AutoInclude();
        modelBuilder.Entity<Person>().Navigation(e => e.Address).AutoInclude();
        modelBuilder.Entity<Person>().Navigation(e => e.Farms).AutoInclude();

        // Note: Farm.Owner and Farm.Address are NOT auto-included because:
        // 1. Farms may have Guid.Empty for PersonId/AddressId (temporary state during creation)
        // 2. AutoInclude would fail when trying to load non-existent entities
        // Use explicit .Include() in queries when needed
        // Note continued:
        // Should be handled anotherway when Guid.Empty as it cannot be applied in FK relationship
        // by using a placeholder before saving and get outputed Guid from DB.

        // Important:
        // Please keep navigation auto-includes here in sync with Repository.cs to ensure
        // consistent behavior across the application.
        modelBuilder.Entity<Farm>().Navigation(e => e.Owner).AutoInclude();
        modelBuilder.Entity<Farm>().Navigation(e => e.Address).AutoInclude();

        modelBuilder.Entity<NatureArea>().Navigation(e => e.Coordinates).AutoInclude();
        modelBuilder.Entity<NatureArea>().Navigation(e => e.Images).AutoInclude();

        // Configure NatureCheckCase Status enum to string conversion
        // Database stores Status as NVARCHAR with CHECK constraint: 'Assigned', 'InProgress', 'Completed', 'Cancelled'
        // Use ValueConverter for more explicit control over conversion
        var statusConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<NatureCheckCaseStatus, string>(
            v => v.ToString(), // Convert enum to string when saving
            v => ParseStatus(v)); // Convert string back to enum when reading
            
        modelBuilder.Entity<NatureCheckCase>()
            .Property(e => e.Status)
            .HasColumnType("nvarchar(50)")
            .HasConversion(statusConverter);
            
        // Configure DateTimeOffset conversion for CreatedAt and AssignedAt
        // Database uses DATETIME2, but EF Core expects DateTimeOffset
        // Convert DateTime (from DATETIME2) to DateTimeOffset when reading
        modelBuilder.Entity<NatureCheckCase>()
            .Property(e => e.CreatedAt)
            .HasConversion(
                v => v.DateTime, // Convert DateTimeOffset to DateTime when saving
                v => new DateTimeOffset(v, TimeSpan.Zero)); // Convert DateTime to DateTimeOffset when reading (assume UTC)
            
        modelBuilder.Entity<NatureCheckCase>()
            .Property(e => e.AssignedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.DateTime : (DateTime?)null, // Convert DateTimeOffset? to DateTime? when saving
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null); // Convert DateTime? to DateTimeOffset? when reading (assume UTC)
    }

    private static NatureCheckCaseStatus ParseStatus(string? value)
    {
        // Handle null or empty values
        if (string.IsNullOrWhiteSpace(value))
        {
            return NatureCheckCaseStatus.Assigned; // Default to Assigned
        }

        // Handle case-insensitive parsing
        // Database CHECK constraint only allows: 'Assigned', 'InProgress', 'Completed', 'Cancelled'
        if (Enum.TryParse<NatureCheckCaseStatus>(value, true, out NatureCheckCaseStatus result))
        {
            // Ensure the parsed value is one of the database-allowed values
            if (result == NatureCheckCaseStatus.Assigned || 
                result == NatureCheckCaseStatus.InProgress || 
                result == NatureCheckCaseStatus.Completed || 
                result == NatureCheckCaseStatus.Cancelled)
            {
                return result;
            }
        }
        // Default to Assigned if invalid or if Draft was parsed (Draft is not in database CHECK constraint)
        return NatureCheckCaseStatus.Assigned;
    }
}
