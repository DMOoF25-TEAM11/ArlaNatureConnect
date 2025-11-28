using ArlaNatureConnect.Domain.Enums;

namespace ArlaNatureConnect.Domain.Entities;

// Purpose: Represents the assignment of a consultant to perform a Nature Check on a specific farm.
// Notes: Stores key audit data so flows can reason about duplicates, ownership and notification state.
public class NatureCheckCase
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public Guid ConsultantId { get; set; }
    public Guid AssignedByPersonId { get; set; }
    public NatureCheckCaseStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AssignedAt { get; set; }
}


