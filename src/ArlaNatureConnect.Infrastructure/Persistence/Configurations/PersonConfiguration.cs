using ArlaNatureConnect.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArlaNatureConnect.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Person entity.
/// </summary>
/// <remarks>
/// Configures the relationship between Person and Farm entities.
/// A Person can own multiple Farms via the Farm.OwnerId foreign key.
/// </remarks>
public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        // Configure one-to-many relationship: Person -> Farms (via Farm.OwnerId)
        builder
            .HasMany(p => p.Farms)
            .WithOne(f => f.Owner)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid accidental data loss
    }
}

