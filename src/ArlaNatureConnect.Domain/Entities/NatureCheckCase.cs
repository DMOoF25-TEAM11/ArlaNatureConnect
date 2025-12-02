using ArlaNatureConnect.Domain.Abstract;
using ArlaNatureConnect.Domain.Enums;

namespace ArlaNatureConnect.Domain.Entities;

// Purpose: Represents the assignment of a consultant to perform a Nature Check on a specific farm.
// Notes: Stores key audit data so flows can reason about duplicates, ownership and notification state.
public class NatureCheckCase : IEntity
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public Guid ConsultantId { get; set; }
    public Guid AssignedByPersonId { get; set; }
    public NatureCheckCaseStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? Priority { get; set; }

    // Audit property - tracks when the record was created in the database.
    // Should not be confused with AssignedAt which represents the actual assignment time.
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }

    // Navigation properties - object references that represent relationships to other entities.
    // They let you navigate and work with related data in code instead of dealing with raw foreign-key values.
    public virtual Farm Farm { get; set; } = null!;
    public virtual Person Consultant { get; set; } = null!;
    public virtual Person AssignedByPerson { get; set; } = null!;

    public static implicit operator Task<object>(NatureCheckCase? v)
    {
        throw new NotImplementedException();
    }

}
