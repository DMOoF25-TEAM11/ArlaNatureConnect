using ArlaNatureConnect.Domain.Entities;
using ArlaNatureConnect.Domain.Enums;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArlaNatureConnect.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for NatureCheckCase entity.
/// </summary>
/// <remarks>
/// Configures type conversions between the domain model and database schema:
/// <list type="bullet">
/// <item><description>Status enum is stored as NVARCHAR(50) with CHECK constraint ('Assigned', 'InProgress', 'Completed', 'Cancelled')</description></item>
/// <item><description>DateTimeOffset properties (CreatedAt, AssignedAt) are stored as DATETIME2 in the database</description></item>
/// </list>
/// The configuration ensures proper conversion when reading from and writing to the database,
/// including handling of nullable values and default status assignment for invalid or missing values.
/// </remarks>
public class NatureCheckCaseConfiguration : IEntityTypeConfiguration<NatureCheckCase>
{
    public void Configure(EntityTypeBuilder<NatureCheckCase> builder)
    {
        // Status enum to string conversion (database: NVARCHAR with CHECK constraint)
        Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<NatureCheckCaseStatus, string> statusConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<NatureCheckCaseStatus, string>(
            v => v.ToString(), // Enum to string when saving
            v => ParseStatus(v)); // String to enum when reading
            
        builder
            .Property(e => e.Status)
            .HasColumnType("nvarchar(50)")
            .HasConversion(statusConverter);
            
        // DateTimeOffset conversion (database: DATETIME2, EF Core: DateTimeOffset)
        builder
            .Property(e => e.CreatedAt)
            .HasConversion(
                v => v.DateTime, // DateTimeOffset to DateTime when saving
                v => new DateTimeOffset(v, TimeSpan.Zero)); // DateTime to DateTimeOffset when reading (UTC)
            
        builder
            .Property(e => e.AssignedAt)
            .HasConversion(
                v => v.HasValue ? v.Value.DateTime : (DateTime?)null, // DateTimeOffset? to DateTime? when saving
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null); // DateTime? to DateTimeOffset? when reading (UTC)
    }

    private static NatureCheckCaseStatus ParseStatus(string? value)
    {
        // Default to Assigned if null or empty
        if (string.IsNullOrWhiteSpace(value))
        {
            return NatureCheckCaseStatus.Assigned;
        }

        // Case-insensitive parsing (database CHECK: 'Assigned', 'InProgress', 'Completed', 'Cancelled')
        if (Enum.TryParse<NatureCheckCaseStatus>(value, true, out NatureCheckCaseStatus result))
        {
            // Validate parsed value matches database-allowed values
            if (result == NatureCheckCaseStatus.Assigned || 
                result == NatureCheckCaseStatus.InProgress || 
                result == NatureCheckCaseStatus.Completed || 
                result == NatureCheckCaseStatus.Cancelled)
            {
                return result;
            }
        }
        // Default to Assigned if invalid (e.g., Draft is not in database CHECK constraint)
        return NatureCheckCaseStatus.Assigned;
    }
}


