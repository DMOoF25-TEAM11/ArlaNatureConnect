using ArlaNatureConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArlaNatureConnect.Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public DbSet<Farm> Farms { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<NatureCheckCase> NatureCheckCases { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Address table name (database uses "Address" not "Addresses")
        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Address");
        });

        modelBuilder.Entity<NatureCheckCase>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Status is stored as NVARCHAR in database (e.g., "Assigned", "InProgress"), so convert enum to/from string
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),  // Convert enum to string for database
                    v => Enum.Parse<Domain.Enums.NatureCheckCaseStatus>(v));  // Convert string from database to enum
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Priority).HasMaxLength(100);
            
            // Convert DateTimeOffset to/from DATETIME2 in database
            entity.Property(e => e.CreatedAt)
                .HasConversion(
                    v => v.DateTime,  // Convert DateTimeOffset to DateTime for database
                    v => new DateTimeOffset(v, TimeSpan.Zero));  // Convert DateTime from database to DateTimeOffset (assume UTC)
            entity.Property(e => e.AssignedAt)
                .HasConversion(
                    v => v.HasValue ? v.Value.DateTime : (DateTime?)null,  // Convert DateTimeOffset? to DateTime? for database
                    v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);  // Convert DateTime? from database to DateTimeOffset?

            // Configure navigation properties
            entity.HasOne(n => n.Farm)
                .WithMany()
                .HasForeignKey(n => n.FarmId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Consultant)
                .WithMany()
                .HasForeignKey(n => n.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(n => n.AssignedByPerson)
                .WithMany()
                .HasForeignKey(n => n.AssignedByPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
