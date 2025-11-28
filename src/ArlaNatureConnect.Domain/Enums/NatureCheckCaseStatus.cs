namespace ArlaNatureConnect.Domain.Enums;

// Purpose: Represents the lifecycle states for a Nature Check Case.
// Notes: Aligns with UC002B so services/UI can reason about whether a case is active or finalised.
public enum NatureCheckCaseStatus
{
    Draft = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}


