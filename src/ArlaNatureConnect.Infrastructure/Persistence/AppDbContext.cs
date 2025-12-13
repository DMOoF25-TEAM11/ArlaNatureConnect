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
        modelBuilder.Entity<Farm>().Navigation(e => e.Owner).AutoInclude();
        modelBuilder.Entity<Farm>().Navigation(e => e.Address).AutoInclude();

        modelBuilder.Entity<NatureArea>().Navigation(e => e.Coordinates).AutoInclude();
        modelBuilder.Entity<NatureArea>().Navigation(e => e.Images).AutoInclude();

        // Ensure EF includes navigation properties for NatureCheckCase so UI bindings to Farm/Consultant work
        modelBuilder.Entity<NatureCheckCase>().Navigation(n => n.Farm).AutoInclude();
        modelBuilder.Entity<NatureCheckCase>().Navigation(n => n.Consultant).AutoInclude();
        // NOTE: AssignedByPerson navigation is NOT auto-included because legacy dbo.NatureCheck does not have AssignedByPersonId

        // Configure NatureCheckCase mapping to match the database table name used in the user's DB
        modelBuilder.Entity<NatureCheckCase>(entity =>
        {
            // The user's database table is named 'NatureCheck'
            entity.ToTable("NatureCheck");

            entity.HasKey(n => n.Id);

            // Map CLR property CreatedAt to SQL column 'Date' if present
            entity.Property(n => n.CreatedAt).HasColumnName("Date");

            // Map ConsultantId to the PersonId column used in the legacy table
            entity.Property(n => n.ConsultantId).HasColumnName("PersonId");

            // Some older DB schemas do not contain these columns; ignore them to avoid SQL errors
            entity.Ignore(n => n.AssignedAt);
            entity.Ignore(n => n.AssignedByPersonId);
            entity.Ignore(n => n.Notes);
            entity.Ignore(n => n.Priority);
            entity.Ignore(n => n.Status);

            // Also ignore the navigation for AssignedByPerson so EF will not create a shadow FK
            entity.Ignore(n => n.AssignedByPerson);

            // Ensure foreign keys are configured where possible
            entity.HasOne(n => n.Farm)
                  .WithMany()
                  .HasForeignKey(n => n.FarmId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Consultant)
                  .WithMany()
                  .HasForeignKey(n => n.ConsultantId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Do not map AssignedByPerson foreign-key because the legacy table does not contain that column.
            // If the column is added to the DB in future, remove the Ignore above and reintroduce the FK mapping.

            // Do not enforce column mappings for other optional properties here; rely on convention or add mappings later if needed.
        });
    }
}
