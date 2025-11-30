namespace ArlaNatureConnect.Core.DTOs;

// Purpose: Captures the data required to assign a Nature Check Case.
// Notes: Keeps service API expressive and allows validation before touching repositories.
public sealed class NatureCheckCaseAssignmentRequest
{
    public Guid FarmId { get; init; }
    public Guid ConsultantId { get; init; }
    public Guid AssignedByPersonId { get; init; }
    public string? Notes { get; init; }
    public string? Priority { get; init; }
    public bool AllowDuplicateActiveCase { get; init; }
}


